using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Threading;
using MPXRemote.Message;

public class OnCompleteLoad : UnityEvent<MPXLibraryImportObject, EventCreateLibaryObject> { }

public class MeshLoader<T> : TaskReceiver<T> where T : TaskReceiver<T>
{
    public static int m_vetex_limit = 65530;
    public string FilePath;


    /// <summary>
    /// 전체 로드 완료 이벤트
    /// </summary>
    public OnCompleteLoad CompleteLoad;


    public override void Init()
    {
        base.Init();
        CompleteLoad = new OnCompleteLoad();
    }

    public void ClearObjs(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
    }

    public virtual void LoadFile(string filePath, EventCreateLibaryObject eCreate = null)
    {

    }

    protected Mesh ConvertToMesh(CustomMesh m, bool RecalcuateNormal)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = m.vertices;
        mesh.normals = m.normals;
        mesh.uv = m.uvs;
        mesh.triangles = m.triangles;

        if (RecalcuateNormal)
        {
            mesh.RecalculateNormals();
        }

        return mesh;
    }

    protected List<Mesh> ConvertToMesh(List<CustomMesh> list, bool RecalcuateNormal)
    {
        List<Mesh> meshs = new List<Mesh>();
        for (int i = 0; i < list.Count; i++)
        {
            meshs.Add(ConvertToMesh(list[i], RecalcuateNormal));
        }

        return meshs;
    }

    protected GameObject CreateGameObject(string objName, Transform parent=null)
    {
        //build objects
        GameObject parentObject = new GameObject(objName);
        if (parent!=null)
        {
            parentObject.transform.SetParent(parent);
        }
        parentObject.transform.localPosition = Vector3.zero;
        parentObject.transform.localScale = Vector3.one;
        //parentObject.tag = tag;

        return parentObject;
    }

    protected Bounds GetBounds(List<Collider> colliders)
    {
        Bounds bounds = new Bounds();

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
        }
        else
            Debug.LogError("Collider not found");

        return bounds;
    }

    /// <summary>
    /// objs들의 부모 설정
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="parent"></param>
    protected void SetParentSubObjects(List<MPXUnityObjectChild> objs, GameObject parent)
    {
        for (int i = 0; i < objs.Count; i++)
        {
            objs[i].transform.SetParent(parent.transform);
        }
    }
}
