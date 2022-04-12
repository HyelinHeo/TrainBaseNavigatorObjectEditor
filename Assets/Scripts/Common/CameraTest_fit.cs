using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest_fit : MonoBehaviour
{
    public Collider targetColl;

    Transform MaxTr;
    Transform MinTr;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 오브젝트 선택시 마다?
    /// </summary>
    void FitCamera()
    {
        float cameraDistance = 2.0f; // Constant factor
        Vector3 objectSizes = targetColl.bounds.max - targetColl.bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        Camera.main.transform.position = targetColl.bounds.center - distance * Camera.main.transform.forward;
        //타겟과의 거리와 전체보기 사이즈 위치 Max와 Min에 대입
        //MaxTr보다 크면 MaxTr, MinTr보다 작으면 MinTr로 위치값 조정 필요.
    }
}
