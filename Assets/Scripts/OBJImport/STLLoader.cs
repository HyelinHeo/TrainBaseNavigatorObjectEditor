using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Threading;
using System.Text;
using MPXRemote.Message;

public enum EN_STL_FILE_FORMAT
{
    TEXT,
    BINARY,
    UNKNOWN
}

public enum EN_TEXT_READ_STATE
{
    SOLID,
    FACET_NORMAL,
    OUTER_LOOP,
    VERTEX,
    END_LOOP,
    END_FACET,
    END_SOLID,
};


public class STLLoader : MeshLoader<STLLoader>
{
    private bool m_is_read = true;

    protected Thread loadThread;

    public delegate void LoadedMesh(List<CustomMesh> mesh_list, EventCreateLibaryObject eCreate);
    public static event LoadedMesh OnLoadedMesh;

    EN_STL_FILE_FORMAT fileFormat;

    Material mat;

    public delegate void OnCheckFile(EN_STL_FILE_FORMAT format, EventCreateLibaryObject eCreate);
    public static event OnCheckFile onCheckFile;

    Regex reg = new Regex("-?[0-9]?\\.[0-9]*e[\\+|\\-][0-9]*");
    string[] result = new string[3];


    private string[] m_text_regex_ptn = new string[]
    {
        "^(\\s*)solid.*",
        "^(\\s*)facet(\\s*)normal(\\s)(\\S*)(\\s)(\\S*)(\\s)(\\S*)",
        "^(\\s*)outer(\\s*)loop",
        "^(\\s*)vertex(\\s*)(\\S*)(\\s*)(\\S*)(\\s*)(\\S*)",
        "^(\\s*)endloop",
        "^(\\s*)endfacet",
        "^(\\s*)endsolid.*",
    };

    public override void Init()
    {
        base.Init();
        OnLoadedMesh += MeshLoader_OnLoadedMesh;
        onCheckFile += STLLoader_onCheckFile;
        mat = Resources.Load<Material>("Materials/defaultMat");
    }

    void MeshLoader_OnLoadedMesh(List<CustomMesh> mesh_list, EventCreateLibaryObject eCreate)
    {
        if (mesh_list == null)
        {
            Debug.LogError("LoadMesh Error: ");
            CompleteLoad.Invoke(null, null);
            return;
        }

        ScheduleTask(new Task(delegate
        {
            OnLoadCompleteMesh(mesh_list, eCreate);

        }));
    }

    void OnLoaded(List<CustomMesh> mesh_list, EventCreateLibaryObject eCreate)
    {
        OnLoadedMesh?.Invoke(mesh_list, eCreate);
    }




    
    List<Material> matlList;
    List<Color> matColorList;
    List<Collider> subObjColliders;
    List<MPXUnityObjectChild> subObjs;
    public void OnLoadCompleteMesh(List<CustomMesh> list, EventCreateLibaryObject eCreate)
    {
        List<Mesh> mesh_list = ConvertToMesh(list, true);
        matlList = new List<Material>();
        matColorList = new List<Color>();
        subObjColliders = new List<Collider>();
        subObjs = new List<MPXUnityObjectChild>();
        //obj.tag = ;

        for (int i = 0; i < mesh_list.Count; i++)
        {
            GameObject child = new GameObject();
            child.name = "obj_" + i;
            MeshFilter filter = child.AddComponent<MeshFilter>();

            MPXUnityObjectChild childObj = child.AddComponent<MPXUnityObjectChild>();
            childObj.SetRenderer(new Material(Shader.Find("Standard (Specular setup)")));

            filter.mesh = mesh_list[i];
            Vector3[] vertices = filter.mesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            int j = 0;
            while (j < uvs.Length)
            {
                uvs[j] = new Vector2(vertices[j].x, vertices[j].z);
                j++;
            }
            filter.mesh.uv = uvs;
            filter.mesh.Optimize();
            subObjColliders.Add(child.AddComponent<BoxCollider>());
            subObjs.Add(childObj);
        }
        mesh_list.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();


        GameObject parentObject = CreateGameObject("STLObject");
        MPXLibraryImportObject libObject = parentObject.AddComponent<MPXLibraryImportObject>();
        libObject.Children = subObjs;

        Bounds bounds = new Bounds();
        bounds = GetBounds(subObjColliders);
        Vector3 pos = bounds.center;
        //pos.y = pos.y - bounds.size.y * 0.5f;//중앙아래
        parentObject.transform.position = pos;
        SetParentSubObjects(subObjs, parentObject);
        parentObject.transform.position = Vector3.zero;

        CompleteLoad.Invoke(libObject, eCreate);
    }

    void OnDestroy()
    {
        if (loadThread != null)
        {
            loadThread.Abort();
        }
    }

    private void STLLoader_onCheckFile(EN_STL_FILE_FORMAT format, EventCreateLibaryObject eCreate)
    {
        fileFormat = format;


        switch (fileFormat)
        {
            case EN_STL_FILE_FORMAT.BINARY:
                loadThread = new Thread(() => loadMeshFromBin(this, eCreate));
                loadThread.Start();
                break;

            case EN_STL_FILE_FORMAT.TEXT:
                loadThread = new Thread(() => loadMeshFromText(this, eCreate));
                loadThread.Start();
                break;

            case EN_STL_FILE_FORMAT.UNKNOWN:
                loadThread = new Thread(() => loadMeshFromBin(this, eCreate));
                loadThread.Start();
                break;

            default:
                Debug.LogError("Please stl format file");
                return;
        }
    }

    public override void LoadFile(string file_path, EventCreateLibaryObject eCreate = null)
    {
        FilePath = file_path;

        if ((FilePath == null) || (FilePath == string.Empty) || !File.Exists(FilePath))
        {
            CompleteLoad.Invoke(null, eCreate);
            return;
        }

        loadThread = new Thread(() => analyzeStlFormat(eCreate));
        loadThread.Start();
    }

    public void OnCheck(EN_STL_FILE_FORMAT format, EventCreateLibaryObject eCreate)
    {
        onCheckFile?.Invoke(format, eCreate);
    }

    void analyzeStlFormat(EventCreateLibaryObject eCreate)
    {
        string first_line = "";
        StreamReader reader = null;
        try
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            reader = new StreamReader(FilePath, Encoding.Default);
            first_line = reader.ReadLine();
            reader.Close();

            stopWatch.Stop();
            Debug.Log(stopWatch.ElapsedMilliseconds);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e);
            reader.Close();

            OnCheck(EN_STL_FILE_FORMAT.UNKNOWN, eCreate);
            return;
        }
        catch (IOException e)
        {
            Debug.LogError(e);
            reader.Close();
            OnCheck(EN_STL_FILE_FORMAT.UNKNOWN, eCreate);
            return;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            reader.Close();
            OnCheck(EN_STL_FILE_FORMAT.UNKNOWN, eCreate);
            return;
        }
        Regex text_mode = new Regex("solid", RegexOptions.IgnoreCase);
        if (text_mode.IsMatch(first_line))
        {
            Debug.Log("This is text format");
            OnCheck(EN_STL_FILE_FORMAT.TEXT, eCreate);
            return;
        }
        Regex bin_mode = new Regex("STL binary", RegexOptions.IgnoreCase);
        if (bin_mode.IsMatch(first_line))
        {
            Debug.Log("This is binary format");
            OnCheck(EN_STL_FILE_FORMAT.BINARY, eCreate);
            return;
        }
        Debug.Log(FilePath + " is Unknown file format");
        OnCheck(EN_STL_FILE_FORMAT.UNKNOWN, eCreate);
        return;
    }

    public void loadMeshFromBin(object loader, EventCreateLibaryObject eCreate)
    {
        STLLoader lo = (STLLoader)loader;

        List<CustomMesh> m_tmp_meshes = new List<CustomMesh>();

        BinaryReader reader;
#if UNITY_WEBPLAYER
			Debug.LogError("Please DO NOT exec WebPlayer mode.");
			return null;
#else
        reader = new BinaryReader(new MemoryStream(System.IO.File.ReadAllBytes(FilePath)));
#endif
        reader.ReadBytes(80);
        List<Vector3> mesh_vertices = new List<Vector3>();
        List<int> mesh_triangles = new List<int>();
        int triangle_count = 0;
        uint tri_count = reader.ReadUInt32();
        for (int i = 0; i < tri_count; i++)
        {
            reader.ReadBytes(12);
            float v3 = reader.ReadSingle();
            float v2 = reader.ReadSingle();
            float v1 = reader.ReadSingle();
            Vector3 vertex1 = new Vector3(v1, v2, v3);
            v3 = reader.ReadSingle();
            v2 = reader.ReadSingle();
            v1 = reader.ReadSingle();
            Vector3 vertex2 = new Vector3(v1, v2, v3);
            v3 = reader.ReadSingle();
            v2 = reader.ReadSingle();
            v1 = reader.ReadSingle();
            Vector3 vertex3 = new Vector3(v1, v2, v3);
            if (m_is_read)
            {
                mesh_vertices.Add(vertex3);
                mesh_vertices.Add(vertex2);
                mesh_vertices.Add(vertex1);
                mesh_triangles.Add(triangle_count++);
                mesh_triangles.Add(triangle_count++);
                mesh_triangles.Add(triangle_count++);
            }
            else
            {
                m_is_read = true;
            }
            reader.ReadInt16();
            if (m_vetex_limit >= mesh_vertices.Count)
            {
                continue;
            }
            lo.addVerticesTriangles(m_tmp_meshes, mesh_vertices, mesh_triangles);
            mesh_vertices.Clear();
            mesh_triangles.Clear();
            triangle_count = 0;

        }
        if (mesh_vertices.Count > 0)
        {
            lo.addVerticesTriangles(m_tmp_meshes, mesh_vertices, mesh_triangles);

            mesh_vertices.Clear();
            mesh_triangles.Clear();
            triangle_count = 0;
        }

        OnLoaded(m_tmp_meshes, eCreate);
    }

    void addVerticesTriangles(
                                    List<CustomMesh> target_mesh,
                                    List<Vector3> vertices,
                                    List<int> triangles)
    {
        CustomMesh mesh = new CustomMesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals();
        target_mesh.Add(mesh);
    }

    public void loadMeshFromText(object loader, EventCreateLibaryObject eCreate)
    {
        STLLoader lo = (STLLoader)loader;

        List<CustomMesh> m_tmp_meshes = new List<CustomMesh>();

        Regex[] regex_array =
            new Regex[7]
            {
                new Regex(m_text_regex_ptn[0], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[1], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[2], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[3], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[4], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[5], RegexOptions.IgnoreCase),
                new Regex(m_text_regex_ptn[6], RegexOptions.IgnoreCase),
            };
        //StreamReader reader = null;
        EN_TEXT_READ_STATE text_state = EN_TEXT_READ_STATE.SOLID;
        string line = null;
        //reader = new StreamReader(FilePath, System.Text.Encoding.Default);
        string[] vertexes = null;
        List<Vector3> mesh_vertices = new List<Vector3>();
        List<int> mesh_triangles = new List<int>();
        int triangle_count = 0;

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        string[] lines = File.ReadAllLines(FilePath);


        int index = -1;
        try
        {
            //while ((line = reader.ReadLine()) != null)
            while (index < lines.Length)
            {
                line = lines[++index];
                switch (text_state)
                {
                    case EN_TEXT_READ_STATE.SOLID:
                        if (!regex_array[0].IsMatch(line))
                        {
                            Debug.LogError("not match! line=" + line);
                            OnLoaded(null, null);
                            return;
                        }
                        text_state = EN_TEXT_READ_STATE.FACET_NORMAL;
                        break;

                    case EN_TEXT_READ_STATE.FACET_NORMAL:
                        if (!regex_array[1].IsMatch(line))
                        {
                            if (regex_array[6].IsMatch(line))
                            {
                                if (mesh_vertices.Count > 0)
                                {
                                    lo.addVerticesTriangles(m_tmp_meshes, mesh_vertices, mesh_triangles);
                                }
                                mesh_vertices.Clear();
                                mesh_triangles.Clear();
                                triangle_count = 0;

                                stopWatch.Stop();
                                Debug.Log("parsing: " + stopWatch.ElapsedMilliseconds);


                                OnLoaded(m_tmp_meshes, eCreate);

                                return;
                            }
                            Debug.LogError("not match! line=" + line);
                            OnLoaded(null, null);
                            return;
                        }
                        text_state = EN_TEXT_READ_STATE.OUTER_LOOP;
                        break;

                    case EN_TEXT_READ_STATE.OUTER_LOOP:
                        //if (!regex_array[2].IsMatch(line))
                        //{
                        //    Debug.LogError("not match! line=" + line);
                        //    OnLoaded(null);
                        //    return;
                        //}
                        text_state = EN_TEXT_READ_STATE.VERTEX;
                        break;

                    case EN_TEXT_READ_STATE.VERTEX:
                        if (!regex_array[3].IsMatch(line))
                        {
                            Debug.LogError("not match! line=" + line);
                            OnLoaded(null, null);
                            return;
                        }
                        // 1st line
                        vertexes = lo.parseVertexes(line);
                        float v1, v2, v3;
                        float.TryParse(vertexes[2], out v1);
                        float.TryParse(vertexes[1], out v2);
                        float.TryParse(vertexes[0], out v3);
                        Vector3 vertex1 = new Vector3(v1, v2, v3);

                        // 2nd line
                        //line = reader.ReadLine();
                        line = lines[++index];
                        //if (!regex_array[3].IsMatch(line))
                        //{
                        //    Debug.LogError("not match! line=" + line);
                        //    OnLoaded(null);
                        //    return;
                        //}
                        vertexes = lo.parseVertexes(line);

                        float.TryParse(vertexes[2], out v1);
                        float.TryParse(vertexes[1], out v2);
                        float.TryParse(vertexes[0], out v3);
                        Vector3 vertex2 = new Vector3(v1, v2, v3);

                        // 3rd line
                        //line = reader.ReadLine();
                        line = lines[++index];
                        //if (!regex_array[3].IsMatch(line))
                        //{
                        //    Debug.LogError("not match! line=" + line);
                        //    OnLoaded(null);
                        //    return;
                        //}
                        vertexes = lo.parseVertexes(line);
                        float.TryParse(vertexes[2], out v1);
                        float.TryParse(vertexes[1], out v2);
                        float.TryParse(vertexes[0], out v3);
                        Vector3 vertex3 = new Vector3(v1, v2, v3);

                        mesh_vertices.Add(vertex3);
                        mesh_vertices.Add(vertex2);
                        mesh_vertices.Add(vertex1);
                        mesh_triangles.Add(triangle_count++);
                        mesh_triangles.Add(triangle_count++);
                        mesh_triangles.Add(triangle_count++);

                        text_state = EN_TEXT_READ_STATE.END_LOOP;
                        break;

                    case EN_TEXT_READ_STATE.END_LOOP:
                        //if (!regex_array[4].IsMatch(line))
                        //{
                        //    Debug.LogError("not match! line=" + regex_array[4]);
                        //    OnLoaded(null);
                        //    return;
                        //}
                        text_state = EN_TEXT_READ_STATE.END_FACET;
                        break;

                    case EN_TEXT_READ_STATE.END_FACET:
                        //if (!regex_array[5].IsMatch(line))
                        //{
                        //    Debug.LogError("not match! line=" + line);
                        //    OnLoaded(null);
                        //    return;
                        //}
                        text_state = EN_TEXT_READ_STATE.FACET_NORMAL;
                        break;

                    default:
                        Debug.LogError("Invalid line = " + line);
                        break;
                }

                if (m_vetex_limit >= mesh_vertices.Count)
                {
                    continue;
                }
                lo.addVerticesTriangles(m_tmp_meshes, mesh_vertices, mesh_triangles);
                mesh_vertices.Clear();
                mesh_triangles.Clear();
                triangle_count = 0;
            }
            //reader.Close();
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError("FileNotFoundException " + e.Message);
            //reader.Close();
            OnLoaded(null, null);
            return;
        }
        catch (IOException e)
        {
            Debug.LogError("IOException " + e.Message);
            //reader.Close();
            OnLoaded(null, null);
            return;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception " + e + " line= " + line);
            //reader.Close();
            OnLoaded(null, null);
            return;
        }
        Debug.Log("mesh_vertices.Count = " + mesh_vertices.Count);

        if (mesh_vertices.Count > 0)
        {
            lo.addVerticesTriangles(m_tmp_meshes, mesh_vertices, mesh_triangles);
            mesh_vertices.Clear();
            mesh_triangles.Clear();
            triangle_count = 0;
        }

        stopWatch.Stop();
        Debug.Log("parsing: " + stopWatch.ElapsedMilliseconds);

        OnLoaded(m_tmp_meshes, eCreate);
    }

    string[] parseVertexes(string src)
    {
        MatchCollection collection = reg.Matches(src);
        if (3 > collection.Count)
        {
            //Debug.LogError("invalid format. Count= " + collection.Count + " src= " + src);
            //OnLoaded(null);
            return null;
        }
        for (int i = 0; i < 3; i++)
        {
            result[i] = collection[i].Value;
        }
        return result;
    }

}
