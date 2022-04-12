using MPXRemote.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPXObject;
using MPXObject.NaviWorkObject;

public class SendEventCreateObjectMyself : MonoBehaviour
{
    public MpxNaviWorkObject.ObjectType ObjectType;
    //public CreateMPXObject CreateObj;
    public Vector3 Pos;
    public Vector3 Rot;
    public Vector3 Size;
    public bool State = true;

    [SerializeField]
    int id = 0;
    public string thisName;

    public int Command;
    public int Request;

    // Start is called before the first frame update
    void Start()
    {
        //ControlScenes.Inst.sendEvent = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Send()
    {
        id++;
        EventCreateObject eCreate = new EventCreateObject();
        eCreate.Command = Command;
        eCreate.Request = Request;
        eCreate.ObjectType = (int)ObjectType;
        //ObjectInfo objInfo = new ObjectInfo();
        //objInfo.ID = id.ToString();
        //objInfo.Name = thisName;
        //objInfo.Position = CreateMPXObject.Vector3ToPoint3(Pos);
        //objInfo.Rotation = CreateMPXObject.Vector3ToPoint3(Rot);
        //objInfo.Size = CreateMPXObject.Vector3ToPoint3(Size);
        //eCreate.ObjInfo = objInfo;

        //CreateObj.OnReceive(eCreate);
    }
}
