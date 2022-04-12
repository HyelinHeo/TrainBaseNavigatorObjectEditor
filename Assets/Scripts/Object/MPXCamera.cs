using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MPXObject;
using MPXObject.NaviWorkObject;
using MPXRemote.Message;
using System;
using MPXObject.NaviObject;

public class MPXCamera : MPXUnityObject
{

    public Camera MyCam;
    public CameraOperate MyCamOper;
    public PostProcessLayer ProcessLayer;
    public PostProcessVolume ProcessVolume;
    AmbientOcclusion Occlusion;
    Vector3 Pos;
    Vector3 Rot;


    public override void Init()
    {
        base.Init();
        Pos = Mytr.position;
        Rot= Mytr.eulerAngles;
        ProcessVolume.sharedProfile.TryGetSettings<AmbientOcclusion>(out Occlusion);
        MPXObjectManager.Inst.ChangeTarget.AddListener(ChangeTarget);
        ControlScene.Inst.ClosedScene.AddListener(CloseScene);
    }

    void CloseScene()
    {
        Mytr.position = Pos;
        Mytr.eulerAngles = Rot;
    }

    private void ChangeTarget(MPXUnityObject obj)
    {
        targetCollider = obj.MyCol;
        CameraFullView();
    }

    public override void Draw(EventCreateLibaryObject obj)
    {
        base.Draw(obj);//추후 수정 필요
        EndProcess(obj);
    }

    public override void Modify(EventCreateLibaryObject obj)
    {
        MpxEditorCamera editCamera = (MpxEditorCamera)obj.ObjInfo;
        if (editCamera.FullScreen)
        {
            CameraFullView();
        }
        EndProcess(obj);
    }

    public override void EndProcess(EventCreateLibaryObject eCreate)
    {
        base.EndProcess(eCreate);
    }


    public Vector3 WorldToScreen(Vector3 worldPos)
    {
        if (MyCam != null)
        {
            return MyCam.WorldToScreenPoint(worldPos);
        }
        return Vector3.zero;
    }

    void ControlShadowVisibility(bool show)
    {
        if (show)
            QualitySettings.shadows = ShadowQuality.All;
        else
            QualitySettings.shadows = ShadowQuality.Disable;
    }

    bool ShadowVisibility()
    {
        if (QualitySettings.shadows==ShadowQuality.All)
            return true;
        else
            return false;
    }

    void ControlLightShadeVisibility(bool show)
    {
        if (show)
            Occlusion.active = true;
        else
            Occlusion.active = false;
    }

    public Collider[] targetCollider;
    public float cameraDistance = 2.5f;
    Bounds bounds;
    void CameraFullView()
    {
        bounds = GetBounds();
        Vector3 objectSizes = bounds.max - bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * MyCam.fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        MyCam.transform.position = (-distance) * MyCam.transform.forward;
    }

    Bounds GetBounds()
    {
        Bounds bounds = new Bounds();

        if (targetCollider != null)
        {
            for (int i = 0; i < targetCollider.Length; i++)
            {
                bounds.Encapsulate(targetCollider[i].bounds);
            }
        }
        else
            Debug.LogError("Collider is null");

        return bounds;
    }

    private void OnDestroy()
    {
        //ControlScenes.Inst.GuiCameraView.OnClick.RemoveListener(ChangeCameraMode);
    }
}
