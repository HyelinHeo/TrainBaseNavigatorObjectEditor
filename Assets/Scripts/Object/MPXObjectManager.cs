using MPXObject;
using MPXRemote.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MPXObjectManager : Singleton<MPXObjectManager>
{
    public Dictionary<string, MPXUnityObject> ObjectsDic = new Dictionary<string, MPXUnityObject>();

    public Vector3 RotateValue;

    MPXUnityObject ThisObject = new MPXUnityObject();

    public class OnChangeObject : UnityEvent<MPXUnityObject> { }
    public OnChangeObject ChangeTarget = new OnChangeObject();

    public override void Init()
    {
        base.Init();
    }

    void OnDestroy()
    {
        Clear();
        ObjectsDic = null;
        ThisObject = null;
    }

    public void Clear()
    {
        if (ObjectsDic!=null)
        {
            ObjectsDic.Clear();
        }
        if (ThisObject!=null)
        {
            ThisObject = null;
        }
    }

    /// <summary>
    /// select object list
    /// </summary>
    public void SetSelectObject(MPXUnityObject obj)
    {
        if (ThisObject==null)
        {
            ThisObject = new MPXUnityObject();
        }
        ThisObject = obj;
        ChangeTarget.Invoke(ThisObject);
    }

    /// <summary>
    /// remove object list
    /// </summary>
    public void RemoveSelectObject()
    {
        ThisObject = null;
    }

    public MPXUnityObject FindSelectMPXObject()
    {
        if (ThisObject!=null)
        {
            return ThisObject;
        }
        else
        {
            //Debug.LogFormat("Select List is {0}, Select List count is {1}, check SelectObject.",SelectObjects,SelectObjects.Count);
            return null;
        }
    }

    public void AddObjectToDic(string code, MPXUnityObject obj)
    {
        if (!ObjectsDic.ContainsKey(code))
            ObjectsDic.Add(code, obj);
        else
            Debug.LogError("the id is already exists.");

    }

    public void RemoveObjectToDic(string code)
    {
        if (ObjectsDic.ContainsKey(code))
            ObjectsDic.Remove(code);
        else
            Debug.LogError("the id is already remove.");
    }

    public MPXUnityObject FindMPXObject(string id)
    {
        if (ObjectsDic.ContainsKey(id))
        {
            Debug.Log(ObjectsDic[id].name + " , " + ObjectsDic[id].Name);
            return ObjectsDic[id];
        }
        Debug.LogError("doesn't exists in Dictionary.");
        return null;
    }

    public bool ContainPartState(string id)
    {
        return ObjectsDic.ContainsKey(id);
    }
}
