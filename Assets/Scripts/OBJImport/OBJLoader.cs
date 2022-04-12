/*
(C) 2015 AARO4130
DO NOT USE PARTS OF, OR THE ENTIRE SCRIPT, AND CLAIM AS YOUR OWN WORK
*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MPXRemote.Message;

public class OBJLoader : MeshLoader<OBJLoader>
{
    Thread loadThread;

    Material[] materialCache = null;


    public static bool splitByMaterial = false;
    public static string[] searchPaths = new string[] { "", "%FileName%_Textures" + Path.DirectorySeparatorChar };

    //public AddColor TestScript;

    //structures
    struct OBJFace
    {
        public string materialName;
        public string meshName;
        public int[] indexes;
    }

    public Vector3 ParseVectorFromCMPS(string[] cmps)
    {
        float x = float.Parse(cmps[1]);
        float y = float.Parse(cmps[2]);
        if (cmps.Length == 4)
        {
            float z = float.Parse(cmps[3]);
            return new Vector3(x, y, z);
        }
        return new Vector2(x, y);
    }
    public Color ParseColorFromCMPS(string[] cmps, float scalar = 1.0f)
    {
        float Kr = float.Parse(cmps[1]) * scalar;
        float Kg = float.Parse(cmps[2]) * scalar;
        float Kb = float.Parse(cmps[3]) * scalar;
        return new Color(Kr, Kg, Kb);
    }

    public string OBJGetFilePath(string path, string basePath, string fileName)
    {
        foreach (string sp in searchPaths)
        {
            string s = sp.Replace("%FileName%", fileName);
            if (File.Exists(basePath + s + path))
            {
                return basePath + s + path;
            }
            else if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
    [SerializeField]
    List<Material> matlList;
    [SerializeField]
    List<Color> matColorList;
    public Material[] LoadMTLFile(string fn)
    {
        Material currentMaterial = null;
        matlList = new List<Material>();
        matColorList = new List<Color>();
        if (matlList.Count > 0)
        {
            matlList.Clear();
        }
        if (matColorList.Count > 0)
        {
            matColorList.Clear();
        }
        FileInfo mtlFileInfo = new FileInfo(fn);
        string baseFileName = Path.GetFileNameWithoutExtension(fn);
        string mtlFileDirectory = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;

        foreach (string ln in File.ReadAllLines(fn))
        {
            string l = ln.Trim().Replace("  ", " ");
            string[] cmps = l.Split(' ');
            string data = l.Remove(0, l.IndexOf(' ') + 1);

            if (cmps[0] == "newmtl")
            {
                currentMaterial = null;
                currentMaterial = new Material(Shader.Find("Standard (Specular setup)"));
                matlList.Add(currentMaterial);
                currentMaterial.name = data;
            }
            else if (cmps[0] == "Kd")
            {
                currentMaterial.SetColor("_Color", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "map_Kd")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                {
                    currentMaterial.SetTexture("_MainTex", TextureLoader.LoadTexture(fpth));
                    Debug.Log(fpth);
                }
            }
            else if (cmps[0] == "map_Bump" || cmps[0] == "map_d")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                {
                    currentMaterial.SetTexture("_BumpMap", TextureLoader.LoadTexture(fpth, true));
                    currentMaterial.EnableKeyword("_NORMALMAP");
                }
            }
            else if (cmps[0] == "Ks")
            {
                currentMaterial.SetColor("_SpecColor", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "Ka")
            {
                currentMaterial.SetColor("_EmissionColor", ParseColorFromCMPS(cmps, 0.05f));
                currentMaterial.EnableKeyword("_EMISSION");
            }
            else if (cmps[0] == "d")
            {
                float visibility = float.Parse(cmps[1]);
                if (visibility < 1)
                {
                    Color temp = currentMaterial.color;

                    temp.a = visibility;
                    currentMaterial.SetColor("_Color", temp);

                    //TRANSPARENCY ENABLER
                    currentMaterial.SetFloat("_Mode", 3);
                    currentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    currentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    currentMaterial.SetInt("_ZWrite", 0);
                    currentMaterial.DisableKeyword("_ALPHATEST_ON");
                    currentMaterial.EnableKeyword("_ALPHABLEND_ON");
                    currentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    currentMaterial.renderQueue = 3000;
                }

            }
            else if (cmps[0] == "Ns")
            {
                float Ns = float.Parse(cmps[1]);
                Ns = (Ns / 1000);
                currentMaterial.SetFloat("_Glossiness", Ns);

            }
        }
        for (int i = 0; i < matlList.Count; i++)
        {
            matColorList.Add(matlList[i].color);
        }
        return matlList.ToArray();
    }
    EventCreateLibaryObject eventCreate;
    public override void LoadFile(string filePath, EventCreateLibaryObject eCreate = null)
    {
        FilePath = filePath;
        if ((FilePath == null) || (FilePath == string.Empty) || !File.Exists(FilePath))
        {
            CompleteLoad.Invoke(null, eCreate);
            return;
        }

        loadThread = new Thread(() => LoadOBJFile(eCreate));
        loadThread.Start();
    }

    public void LoadOBJFile(EventCreateLibaryObject eCreate)
    {
        string meshName = Path.GetFileNameWithoutExtension(FilePath);

        bool hasNormals = false;
        //OBJ LISTS
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //UMESH LISTS
        List<Vector3> uvertices = new List<Vector3>();
        List<Vector3> unormals = new List<Vector3>();
        List<Vector2> uuvs = new List<Vector2>();
        //MESH CONSTRUCTION
        List<string> materialNames = new List<string>();
        List<string> objectNames = new List<string>();
        Dictionary<string, int> hashtable = new Dictionary<string, int>();
        List<OBJFace> faceList = new List<OBJFace>();
        string cmaterial = "";
        string cmesh = "default";
        //CACHE
        materialCache = null;
        //save this info for later
        FileInfo OBJFileInfo = new FileInfo(FilePath);
        foreach (string ln in File.ReadAllLines(FilePath))
        {
            if (ln.Length > 0 && ln[0] != '#')
            {
                string l = ln.Trim().Replace("  ", " ");
                string[] cmps = l.Split(' ');
                string data = l.Remove(0, l.IndexOf(' ') + 1);

                if (cmps[0] == "mtllib")
                {
                    //load cache
                    string pth = OBJGetFilePath(data, OBJFileInfo.Directory.FullName + Path.DirectorySeparatorChar, meshName);
                    if (pth != null)
                    {
                        ScheduleTask(new Task(delegate
                        {
                            materialCache = LoadMTLFile(pth);
                        }));
                    }

                }
                else if ((cmps[0] == "g" || cmps[0] == "o") && splitByMaterial == false)
                {
                    cmesh = data;
                    if (!objectNames.Contains(cmesh))
                    {
                        objectNames.Add(cmesh);
                    }
                }
                else if (cmps[0] == "usemtl")
                {
                    cmaterial = data;
                    if (!materialNames.Contains(cmaterial))
                    {
                        materialNames.Add(cmaterial);
                    }

                    if (splitByMaterial)
                    {
                        if (!objectNames.Contains(cmaterial))
                        {
                            objectNames.Add(cmaterial);
                        }
                    }
                }
                else if (cmps[0] == "v")
                {
                    //VERTEX
                    vertices.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vn")
                {
                    //VERTEX NORMAL
                    normals.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vt")
                {
                    //VERTEX UV
                    uvs.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "f")
                {
                    int[] indexes = new int[cmps.Length - 1];
                    for (int i = 1; i < cmps.Length; i++)
                    {
                        string felement = cmps[i];
                        int vertexIndex = -1;
                        int normalIndex = -1;
                        int uvIndex = -1;
                        if (felement.Contains("//"))
                        {
                            //doubleslash, no UVS.
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (felement.Count(x => x == '/') == 2)
                        {
                            //contains everything
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (!felement.Contains("/"))
                        {
                            //just vertex inedx
                            vertexIndex = int.Parse(felement) - 1;
                        }
                        else
                        {
                            //vertex and uv
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                        }
                        string hashEntry = vertexIndex + "|" + normalIndex + "|" + uvIndex;
                        if (hashtable.ContainsKey(hashEntry))
                        {
                            indexes[i - 1] = hashtable[hashEntry];
                        }
                        else
                        {
                            //create a new hash entry
                            indexes[i - 1] = hashtable.Count;
                            hashtable[hashEntry] = hashtable.Count;
                            uvertices.Add(vertices[vertexIndex]);
                            if (normalIndex < 0 || (normalIndex > (normals.Count - 1)))
                            {
                                unormals.Add(Vector3.zero);
                            }
                            else
                            {
                                hasNormals = true;
                                unormals.Add(normals[normalIndex]);
                            }
                            if (uvIndex < 0 || (uvIndex > (uvs.Count - 1)))
                            {
                                uuvs.Add(Vector2.zero);
                            }
                            else
                            {
                                uuvs.Add(uvs[uvIndex]);
                            }

                        }
                    }
                    if (indexes.Length < 5 && indexes.Length >= 3)
                    {
                        OBJFace f1 = new OBJFace();
                        f1.materialName = cmaterial;
                        f1.indexes = new int[] { indexes[0], indexes[1], indexes[2] };
                        f1.meshName = (splitByMaterial) ? cmaterial : cmesh;
                        faceList.Add(f1);
                        if (indexes.Length > 3)
                        {

                            OBJFace f2 = new OBJFace();
                            f2.materialName = cmaterial;
                            f2.meshName = (splitByMaterial) ? cmaterial : cmesh;
                            f2.indexes = new int[] { indexes[2], indexes[3], indexes[0] };
                            faceList.Add(f2);
                        }
                    }
                }
            }
        }

        ScheduleTask(new Task(delegate
        {
            // mtl 항목이 없을경우
            if (materialNames.Count.Equals(0))
            {
                materialNames.Add("");
            }
            //parentObject.tag = 

            //LISTS FOR REORDERING
            Dictionary<int, List<Vector3>> processedVertices = new Dictionary<int, List<Vector3>>();
            Dictionary<int, List<Vector3>> processedNormals = new Dictionary<int, List<Vector3>>();
            Dictionary<int, List<Vector2>> processedUVs = new Dictionary<int, List<Vector2>>();
            Dictionary<int, List<int>> processedIndexes = new Dictionary<int, List<int>>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            //POPULATE MESH
            Dictionary<int, string> meshMaterialNames = new Dictionary<int, string>();

            OBJFace[] ofaces = faceList.ToArray();
            int meshIndex = 0;
            foreach (string mn in materialNames)
            {
                OBJFace[] faces = ofaces.Where(x => x.materialName == mn).ToArray();
                if (faces.Length > 0)
                {
                    if (processedVertices.ContainsKey(meshIndex))
                    {
                        meshIndex++;
                    }
                    processedVertices.Add(meshIndex, new List<Vector3>());

                    for (int i = 0; i < faces.Length; i++)
                    {
                        if (!meshMaterialNames.ContainsKey(meshIndex))
                        {
                            meshMaterialNames.Add(meshIndex, mn);
                        }

                        int[] indices = faces[i].indexes;
                        for (int k = 0; k < indices.Length; k++)
                        {
                            int idx = indices[k];

                            //build remap table
                            if (!remapTable.ContainsKey(idx))
                            {
                                if (!processedVertices.ContainsKey(meshIndex))
                                {
                                    processedVertices.Add(meshIndex, new List<Vector3>());
                                }
                                if (!processedNormals.ContainsKey(meshIndex))
                                {
                                    processedNormals.Add(meshIndex, new List<Vector3>());
                                }
                                if (!processedUVs.ContainsKey(meshIndex))
                                {
                                    processedUVs.Add(meshIndex, new List<Vector2>());
                                }

                                processedVertices[meshIndex].Add(uvertices[idx]);
                                processedNormals[meshIndex].Add(unormals[idx]);
                                processedUVs[meshIndex].Add(uuvs[idx]);

                                remapTable.Add(idx, processedVertices[meshIndex].Count - 1);
                            }


                            if (!processedIndexes.ContainsKey(meshIndex))
                            {
                                processedIndexes.Add(meshIndex, new List<int>());
                            }
                            processedIndexes[meshIndex].Add(remapTable[idx]);

                        }

                        if (m_vetex_limit <= processedVertices[meshIndex].Count)
                        {
                            meshIndex++;
                            processedVertices.Add(meshIndex, new List<Vector3>());
                            remapTable.Clear();
                        }
                    }
                }
                else
                {
                    //에러
                    Debug.LogError("loader error: " + mn);
                    CompleteLoad.Invoke(null, eCreate);
                    return;
                }
            }
            remapTable.Clear();

            //apply stuff
            Material[] processedMaterials = new Material[processedIndexes.Count];
            List<Collider> subObjColliders = new List<Collider>();
            List<MPXUnityObjectChild> subObjs = new List<MPXUnityObjectChild>();
            for (int i = 0; i < processedIndexes.Count; i++)
            {
                GameObject subObj = CreateGameObject("obj_" + i);

                Mesh m = new Mesh();
                m.name = subObj.name;

                m.vertices = processedVertices[i].ToArray();
                m.normals = processedNormals[i].ToArray();
                m.uv = processedUVs[i].ToArray();

                int[] triangles = processedIndexes[i].ToArray();
                m.SetTriangles(triangles, 0);


                if (!hasNormals)
                {
                    m.RecalculateNormals();
                }
                m.RecalculateBounds();
                m.Optimize();

                MeshFilter mf = subObj.AddComponent<MeshFilter>();
                //MeshRenderer mr = subObj.AddComponent<MeshRenderer>();
                mf.mesh = m;

                if (materialCache == null)
                {
                    processedMaterials[i] = new Material(Shader.Find("Standard (Specular setup)"));
                }
                else
                {
                    Material mfn = Array.Find(materialCache, x => x.name == meshMaterialNames[i]);
                    if (mfn == null)
                    {
                        processedMaterials[i] = new Material(Shader.Find("Standard (Specular setup)"));
                    }
                    else
                    {
                        processedMaterials[i] = mfn;
                    }

                }
                processedMaterials[i].name = meshMaterialNames[i];

                //mr.material = processedMaterials[i];

                MPXUnityObjectChild child = subObj.AddComponent<MPXUnityObjectChild>();
                child.SetRenderer(processedMaterials[i]);
                subObjColliders.Add(subObj.AddComponent<BoxCollider>());

                subObjs.Add(child);
            }


            GameObject parentObject = CreateGameObject(meshName);
            MPXLibraryImportObject libObject = parentObject.AddComponent<MPXLibraryImportObject>();
            libObject.Children = subObjs;
            //libObject.MatColorList = matColorList;
            //libObject.MatList = matlList;
            Bounds bounds = new Bounds();
            bounds = GetBounds(subObjColliders);
            Vector3 pos = bounds.center;
            //pos.y = pos.y - bounds.size.y * 0.5f;//중앙아래
            parentObject.transform.position = pos;
            SetParentSubObjects(subObjs, parentObject);
            parentObject.transform.position = Vector3.zero;

            CompleteLoad.Invoke(libObject, eCreate);


            processedIndexes.Clear();
            processedNormals.Clear();
            processedUVs.Clear();
            processedVertices.Clear();
            meshMaterialNames.Clear();

        }));
    }

    Mesh AddMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        return mesh;
    }
}