using MPXRemote.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPXLibraryImportObject : MPXUnityObject
{//rgbColor 값
    [SerializeField]
    Color rgbColor;
    [SerializeField]
    MPXObject.Color mpxColor;

    public List<Material> MatList;
    public List<Color> MatColorList;

    public override void Init()
    {
        Mytr = this.transform;
        MyCol = GetComponentsInChildren<Collider>();
        base.Init();

        if (Children != null)
        {
            MatList = new List<Material>();
            MatColorList = new List<Color>();
            for (int i = 0; i < Children.Count; i++)
            {
                MatList.AddRange(Children[i].Materials);
                MatColorList.AddRange(Children[i].DefaultColors);
            }
        }
        else
        {
            Children = new List<MPXUnityObjectChild>();
        }

    }

    public float RotSpeed = 3.0f;
    float mouseX;
    float mouseY;

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            Mytr.Rotate(Vector3.down * RotSpeed * mouseX, Space.World);
            Mytr.Rotate(Vector3.right * RotSpeed * mouseY, Space.World);
            MPXObjectManager.Inst.RotateValue = Mytr.eulerAngles;
        }
    }

    public override void Draw(EventCreateLibaryObject obj)
    {
        base.Draw(obj);
        Mytr.eulerAngles = MPXObjectManager.Inst.RotateValue;
        ChangeObjectColor(obj.ObjInfo.MyColor);
        EndProcess(obj);
    }

    public override void Modify(EventCreateLibaryObject obj)
    {
        Name = name;
        Mytr.eulerAngles = MPXObjectManager.Inst.RotateValue;
        Mytr.localScale = CreateObject.Point3ToVector3(obj.ObjInfo.Size);
        this.name = name;

        ChangeObjectColor(obj.ObjInfo.MyColor);
        EndProcess(obj);
    }

    /// <summary>
    /// 윈폼에서 요청 삭제시
    /// </summary>
    /// <param name="obj"></param>
    public override void Delete(EventCreateLibaryObject obj)
    {
        base.Delete(obj);
    }

    /// <summary>
    /// 내부 삭제시
    /// </summary>
    public override void Delete()
    {
        base.Delete();
    }

    public override void EndProcess(EventCreateLibaryObject eCreate)
    {
        base.EndProcess(eCreate);
    }

    public void AddColorAtObj()
    {
        if (MatColorList != null)
        {
            for (int i = 0; i < MatColorList.Count; i++)
            {
                MatList[i].color = (MatColorList[i] + rgbColor) * 0.5f;
            }
        }
    }

    public void ChangeObjectColor(MPXObject.Color color)
    {
        byte r = byte.Parse(color.Red.ToString());
        byte g = byte.Parse(color.Green.ToString());
        byte b = byte.Parse(color.Blue.ToString());
        byte a = byte.Parse(color.Alpha.ToString());
        rgbColor = new Color32(r, g, b, a);
        AddColorAtObj();
    }

    public MPXObject.Color UnityColorToMpxColor(Color color)
    {
        mpxColor = new MPXObject.Color((int)color.r, (int)color.g, (int)color.b, (int)color.a);
        return mpxColor;
    }
}
