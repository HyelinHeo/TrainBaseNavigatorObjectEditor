using MPXObject;
using MPXRemote.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPXObject.NaviObject;
using MpxUnityObject;

public class CreateObject : Singleton<CreateObject>
{
    public const int CREATE = 0;
    public const int MODIFY = 1;
    /// <summary>
    /// 테스트용
    /// </summary>
    //public MPXUnityObject TestModel;
    public MPXCamera MainCamera;

    MPXLibraryNomalObject[] Objects;
    STLLoader stlLoader;
    OBJLoader objLoader;
    MUOLoader muoLoader;

    public MPXUnityObject currentObject;

    public override void Init()
    {
        base.Init();
        MainCamera.Init();
        ReceiverManager.Inst.OnReceiveP.AddListener(OnReceiveCreate);

        STLLoader.Inst.Init();
        OBJLoader.Inst.Init();
        MUOLoader.Inst.Init();

        stlLoader = STLLoader.Inst;
        stlLoader.CompleteLoad.AddListener(CompleteLoad);

        objLoader = OBJLoader.Inst;
        objLoader.CompleteLoad.AddListener(CompleteLoad);

        muoLoader = MUOLoader.Inst;
        muoLoader.CompleteLoad.AddListener(CompleteLoad);
    }

    private void CompleteLoad(MPXLibraryImportObject obj, EventCreateLibaryObject eCreate)
    {
        if (obj == null)
            SenderManager.Inst.EndErrorProcess(eCreate.ID);
        else
        {
            currentObject = obj;
            obj.Init();
            obj.Draw(eCreate);

            MpxNaviObjectImport importObj = (MpxNaviObjectImport)eCreate.ObjInfo;
            if (importObj.Type != MpxNaviObjectImport.FileType.MUO)
            {
                string saveFileName = importObj.SaveFolder + "\\" + importObj.ID + MpxUnityObjectFile.EXT;
                importObj.FilePath = saveFileName;
                importObj.Type = MpxNaviObjectImport.FileType.MUO;
                eCreate.ObjInfo = importObj;

                MUOExporter.Inst.Save(obj, saveFileName);
            }
        }
    }

    private void OnReceiveCreate(Protocol p)
    {
        if (p.PType == Protocol.ProtocolType.EventCreateLibaryObject)
        {
            EventCreateLibaryObject eCreate = (EventCreateLibaryObject)p;

            if (eCreate.Command == EventCreateLibaryObject.COMMAND_CREATE)
            {
                DisableAllNomalObject();
                if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.NORMAL)
                {
                    CreateNomalObject(eCreate);
                }
                else if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.IMPORT)
                {
                    CreateImportObject(eCreate);
                }
            }
            else if (eCreate.Command == EventCreateLibaryObject.COMMAND_EDIT)
            {
                if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.CAMERA)
                {
                    MainCamera.Modify(eCreate);
                }
                else if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.NORMAL)
                {
                    if (currentObject != null)
                    {
                        currentObject.Modify(eCreate);
                    }
                }
                else if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.IMPORT)
                {
                    currentObject.Modify(eCreate);
                }
            }
            else if (eCreate.Command == EventCreateLibaryObject.COMMAND_DELETE)
            {
                if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.NORMAL)
                {
                    DisableAllNomalObject();
                }
                else if (eCreate.ObjInfo.MyType == MpxNaviObject.ObjectType.IMPORT)
                {
                    if (currentObject != null)
                    {
                        Destroy(currentObject.gameObject);
                    }
                }
                SenderManager.Inst.EndProcess(p.ID);
            }
        }
    }

    void CreateNomalObject(EventCreateLibaryObject eCreate)
    {
        MpxNaviObjectNormal obj = (MpxNaviObjectNormal)eCreate.ObjInfo;
        currentObject = Objects[(int)obj.PrimitiType];
        currentObject.gameObject.SetActive(true);
        currentObject.Init();
        currentObject.Draw(eCreate);
    }

    void CreateImportObject(EventCreateLibaryObject eCreate)
    {
        MpxNaviObjectImport obj = (MpxNaviObjectImport)eCreate.ObjInfo;
        string path = obj.FilePath;

        Debug.Log(path);
        if (obj.Type == MpxNaviObjectImport.FileType.OBJ)
        {
            objLoader.LoadFile(path, eCreate);
        }
        else if (obj.Type == MpxNaviObjectImport.FileType.STL)
        {
            stlLoader.LoadFile(path, eCreate);
        }
        else if (obj.Type == MpxNaviObjectImport.FileType.MUO)
        {
            muoLoader.LoadFile(path, eCreate);
        }
        //currentObject = Objects[(int)obj.PrimitiType];
        //currentObject.gameObject.SetActive(true);
        //currentObject.Init();
        //currentObject.Draw(eCreate);
        //MPXObjectManager.Inst.SetSelectObject(currentObject);
    }

    void DisableAllNomalObject()
    {
        if (Objects != null && Objects.Length > 0)
        {
            for (int i = 0; i < Objects.Length; i++)
            {
                Objects[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetObjectList(MPXLibraryNomalObject[] objs)
    {
        Objects = objs;
    }

    public void ImportFBXObject()
    {
        //if (Target!=null)
        //{
        //    Target.Delete();
        //}
        //GameObject go= Instantiate(TestModel);
        //MPXLibraryObject libraryObject = go.AddComponent<MPXLibraryObject>();
        //libraryObject.Init();
        ////libraryObject.Draw();
    }



    /// <summary>
    /// Point3 Axis Transform Vector3
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector3 Point3ToVector3(Point3 point)
    {
        return new Vector3(point.X, point.Y, point.Z);
    }

    /// <summary>
    /// Vector3 Axis Transform Point3
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Point3 Vector3ToPoint3(Vector3 vector)
    {
        return new Point3(vector.x, vector.y, vector.z);
    }
}
