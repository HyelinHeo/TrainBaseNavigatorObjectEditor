using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPXObject;
using MPXCommon;
using System;

namespace MpxUnityObject
{
    [Serializable]
    public class MpxMeshObject : MpxObject
    {
        public Point3[] Vertices;
        public Point3[] Normals;
        public Point2[] Uvs;
        public int[] Triangles;

        public string MeshName;

        public MpxMaterial[] Materials;

        public List<MpxMeshObject> Children;

        public MpxMeshObject() { }

        //public void AddSubMesh(int[] triangles, Material mat)
        //{
        //    if (subMeshes == null)
        //        subMeshes = new List<MpxSubMesh>();

        //    MpxSubMesh subMesh = new MpxSubMesh();
        //    subMesh.Mat = MpxMaterial.ToColor(mat.color);
        //    sub

        //    subMeshes.Add()

        //}

        public List<MPXObject.Color> GetColors()
        {
            List<MPXObject.Color> colors = new List<MPXObject.Color>();

            if (Materials != null)
            {
                for (int i = 0; i < Materials.Length; i++)
                {
                    colors.Add(Materials[i].MainColor);
                }
            }
            return colors;
        }

        public void AddChild(MpxMeshObject obj)
        {
            if (Children == null)
                Children = new List<MpxMeshObject>();

            Children.Add(obj);
        }

        void SetVectors(Vector3[] vectors, out Point3[] points)
        {
            points = new Point3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                points[i] = new Point3(vectors[i].x, vectors[i].y, vectors[i].z);
            }
        }

        void SetVectors(Vector2[] vectors, out Point2[] points)
        {
            points = new Point2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                points[i] = new Point2(vectors[i].x, vectors[i].y);
            }
        }

        public Vector3[] ToVector3(Point3[] points)
        {
            Vector3[] vectors = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vectors[i] = new Vector3(points[i].X, points[i].Y, points[i].Z);
            }
            return vectors;
        }

        public Vector2[] ToVector2(Point2[] points)
        {
            Vector2[] vectors = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vectors[i] = new Vector2(points[i].X, points[i].Y);
            }
            return vectors;
        }

        public void SetVertices(Vector3[] vertices)
        {
            SetVectors(vertices, out Vertices);
        }

        public Vector3[] GetVertices()
        {
            return ToVector3(Vertices);
        }


        public void SetNormals(Vector3[] normals)
        {
            SetVectors(normals, out Normals);
        }

        public Vector3[] GetNormals()
        {
            return ToVector3(Normals);
        }

        public void SetUvs(Vector2[] uvs)
        {
            SetVectors(uvs, out Uvs);
        }

        public Vector2[] GetUvs()
        {
            return ToVector2(Uvs);
        }

        public void SetTriangles(int[] triangles)
        {
            Triangles = triangles;
        }

        public int[] GetTriangles()
        {
            return Triangles;
        }

        public void SetColors(UnityEngine.Color[] colors)
        {
            if (Materials != null && Materials.Length >= colors.Length)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    Materials[i].MainColor = MpxMaterial.ToMpxColor(colors[i]);
                    Debug.Log(colors[i]);
                }
            }
        }

        public void SetMaterials(Material[] materials)
        {
            Materials = new MpxMaterial[materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    Materials[i] = new MpxMaterial(materials[i]);
                }
            }
        }

        public void SetMeshFilter(ref MeshFilter mf)
        {
            Mesh mesh = new Mesh();
            mesh.name = MeshName;
            mesh.vertices = GetVertices();
            mesh.normals = GetNormals();
            mesh.uv = GetUvs();
            mesh.triangles = GetTriangles();

            mf.mesh = mesh;
        }

        public void SetMeshRenderer(ref MeshRenderer ren)
        {
            Material[] mats = new Material[Materials.Length];
            for (int i = 0; i < Materials.Length; i++)
            {
                mats[i] = Materials[i].ToMaterial();
            }
            ren.sharedMaterials = mats;
        }
    }
}