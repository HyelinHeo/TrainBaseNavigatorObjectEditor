using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MPXObject;
using MPXRemote.Message;

public class MPXUnityObject : MonoBehaviour
{
    public Collider[] MyCol;
    //public LayerMask MyLayer;
    public Transform Mytr;

    public string ID;
    public string Name;

    public List<MPXUnityObjectChild> Children;

    public virtual void Init()
    {
        MPXObjectManager.Inst.SetSelectObject(this);
    }

    public Transform SetPointsIntoTransform(Point3 pos, Point3 rot, Point3 size)
    {
        Mytr.position = CreateObject.Point3ToVector3(pos);
        Mytr.eulerAngles = CreateObject.Point3ToVector3(rot);
        Mytr.localScale = CreateObject.Point3ToVector3(size);

        return Mytr;
    }

    public Transform SetTransform(Transform tr)
    {
        Mytr = tr;
        return Mytr;
    }

    public Vector3 GetCenter()
    {
        if (MyCol != null)
        {
            return GetBounds().center;
        }
        return Vector3.zero;
    }

    public Vector3 GetCenterForward()
    {
        if (MyCol != null)
        {
            return GetBounds().center + transform.forward * GetBounds().size.z / 2;
        }
        return Vector3.zero;
    }

    Bounds GetBounds()
    {
        Bounds bounds = new Bounds();

        if (MyCol != null)
        {
            for (int i = 0; i < MyCol.Length; i++)
            {
                bounds.Encapsulate(MyCol[i].bounds);
            }
        }
        else
            Debug.LogError("Collider is null");

        return bounds;
    }

    public virtual void Draw(EventCreateLibaryObject obj)
    {
        SetValue(obj.ObjInfo.ID, obj.ObjInfo.Name, obj.ObjInfo.Position, obj.ObjInfo.Rotation, obj.ObjInfo.Size);
        ControlScene.Inst.FadeImage.enabled = false;
    }

    public virtual void Modify(EventCreateLibaryObject obj)
    {
        SetValue(obj.ObjInfo.Name, obj.ObjInfo.Position, obj.ObjInfo.Rotation, obj.ObjInfo.Size);
    }

    void SetValue(string id, string name, Point3 pos, Point3 rot, Point3 size)
    {
        ID = id;
        Name = name;
        Mytr = SetPointsIntoTransform(pos, rot, size);
        this.name = name;
    }

    void SetValue(string name, Point3 pos, Point3 rot, Point3 size)
    {
        Name = name;
        Mytr = SetPointsIntoTransform(pos, rot, size);
        this.name = name;
    }

    public virtual void Delete(EventCreateLibaryObject obj)
    {
        DestroyThisObject();
        EndProcess(obj);
    }

    public virtual void Delete()
    {
        DestroyThisObject();
    }

    void DestroyThisObject()
    {
        Destroy(this.gameObject);
        Debug.Log("Delete Object." + Name);
    }

    /// <summary>
    /// 윈폼에서 요청한 값 전송 시
    /// send value which requested by Winform
    /// </summary>
    public virtual void EndProcess(EventCreateLibaryObject eCreate) {
        SenderManager.Inst.EndProcess(eCreate.ID);
    }

    //public virtual void SetLayer()
    //{
    //    Utility.SetLayer(gameObject, MyLayer);
    //}

    //public void SetIgnoreLayer()
    //{
    //    Utility.SetLayerIgnore(gameObject);
    //}
}
