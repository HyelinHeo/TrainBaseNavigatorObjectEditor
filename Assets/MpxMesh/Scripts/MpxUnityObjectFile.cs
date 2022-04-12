using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MPXFile;
using MPXObject;

namespace MpxUnityObject
{
    [Serializable]
    public class MpxUnityObjectFile : CustomFile
    {
        public const string EXT = ".muo";

        public MpxMeshObject Object;

        public MpxUnityObjectFile() { }
        public MpxUnityObjectFile(MPXUnityObject obj)
        {
            Name = obj.name;
            Object = ToMpxMeshObject(null);

            List<MPXUnityObjectChild> children = obj.Children;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].gameObject != obj)
                {
                    Object.AddChild(ToMpxMeshObject(children[i]));
                }
            }
        }

        //void Add(MpxMeshObject obj)
        //{
        //    if (Objects == null)
        //        Objects = new List<MpxMeshObject>();

        //    Objects.Add(obj);
        //}


        //public bool Add(MeshFilter mf, MeshRenderer ren, Collider col)
        //{
        //    MpxMeshObject obj = ToMpxMeshObject(mf, ren, col);
        //    if (obj != null)
        //    {
        //        Add(obj);
        //        return true;
        //    }
        //    return false;
        //}

        public static MpxMeshObject ToMpxMeshObject(MPXUnityObjectChild obj)
        {
            MpxMeshObject newObj = new MpxMeshObject();
            newObj.Position = Point3.Zero;
            newObj.Rotation = Point3.Zero;
            newObj.Size = Point3.One;

            if (obj == null) return newObj;

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            MeshRenderer ren = obj.GetComponent<MeshRenderer>();
            BoxCollider col = obj.GetComponent<BoxCollider>();

            if (mf != null && ren != null && col != null)
            {
                newObj.Name = mf.name;
                newObj.MeshName = mf.mesh.name;

                Vector3 pos = col.transform.localPosition;
                Vector3 rot = col.transform.localEulerAngles;
                Vector3 size = col.transform.localScale;
                newObj.Position = new Point3(pos.x, pos.y, pos.z);
                newObj.Rotation = new Point3(rot.x, rot.y, rot.z);
                newObj.Size = new Point3(size.x, size.y, size.z);

                Mesh m = mf.mesh;
                Material[] mats = ren.sharedMaterials;

                newObj.SetVertices(m.vertices);
                newObj.SetNormals(m.normals);
                newObj.SetUvs(m.uv);
                newObj.SetTriangles(m.triangles);
                newObj.SetMaterials(mats);
                newObj.SetColors(obj.DefaultColors);

            }
            return newObj;
        }

        //public static MpxMeshObject ToMpxMeshObject(MeshFilter mf, MeshRenderer ren, Collider col)
        //{
        //    MpxMeshObject newObj = new MpxMeshObject();
        //    newObj.Position = Point3.Zero;
        //    newObj.Rotation = Point3.Zero;
        //    newObj.Size = Point3.One;

        //    if (mf != null && ren != null && col != null)
        //    {
        //        newObj.Name = mf.name;
        //        newObj.MeshName = mf.mesh.name;

        //        Vector3 pos = col.transform.localPosition;
        //        Vector3 rot = col.transform.localEulerAngles;
        //        Vector3 size = col.transform.localScale;
        //        newObj.Position = new Point3(pos.x, pos.y, pos.z);
        //        newObj.Rotation = new Point3(rot.x, rot.y, rot.z);
        //        newObj.Size = new Point3(size.x, size.y, size.z);

        //        Mesh m = mf.mesh;
        //        Material[] mats = ren.sharedMaterials;

        //        newObj.SetVertices(m.vertices);
        //        newObj.SetNormals(m.normals);
        //        newObj.SetUvs(m.uv);
        //        newObj.SetTriangles(m.triangles);
        //        newObj.SetMaterials(mats);

        //    }
        //    return newObj;
        //}

        public static T ToGameObject<T>(MpxUnityObjectFile file) where T : MPXUnityObject
        {
            if (file != null)
            {
                GameObject go = new GameObject();
                go.name = file.Name;
                T obj = go.AddComponent<T>();

                if (file.Object != null)
                {
                    ToGameObject(file.Object, go.transform);
                }
                return obj;
            }
            return null;
        }

        public static void ToGameObject(MpxMeshObject obj, Transform parent)
        {
            if (obj != null)
            {
                List<MpxMeshObject> objects = obj.Children;
                for (int i = 0; i < objects.Count; i++)
                {
                    MpxMeshObject meshObj = objects[i];

                    GameObject newMeshGo = new GameObject();
                    newMeshGo.transform.SetParent(parent);
                    newMeshGo.transform.localPosition = new Vector3(meshObj.Position.X, meshObj.Position.Y, meshObj.Position.Z);
                    newMeshGo.transform.localEulerAngles = new Vector3(meshObj.Rotation.X, meshObj.Rotation.Y, meshObj.Rotation.Z);
                    newMeshGo.transform.localScale = new Vector3(meshObj.Size.X, meshObj.Size.Y, meshObj.Size.Z);

                    newMeshGo.name = meshObj.Name;
                    MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
                    MeshRenderer ren = newMeshGo.AddComponent<MeshRenderer>();

                    meshObj.SetMeshFilter(ref mf);
                    meshObj.SetMeshRenderer(ref ren);

                    MPXUnityObjectChild child = newMeshGo.AddComponent<MPXUnityObjectChild>();
                    child.SetRenderer(ren);

                    newMeshGo.AddComponent<BoxCollider>();

                    if (meshObj.Children != null && meshObj.Children.Count > 0)
                    {
                        ToGameObject(meshObj, newMeshGo.transform);
                    }
                }
            }
        }

    }
}
