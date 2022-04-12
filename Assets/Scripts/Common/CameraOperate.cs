/**
 *  the author: D2og
 *  date: 2019-03-06
 *  what it does: lens control (mimic the Unity editor)
 *  how to use it: just put the script on the camera
 *  operation method:   1. Right click and press + mouse to move so that the lens to rotate
 *                      2. Press the mouse wheel + mouse to move so that the lens to translation
 *                      3. Right mouse button + keyboard w s a d (+leftShift) so that the lens to move
 *                      4. the mouse wheel rolling so that the lens forward and backward
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOperate : MonoBehaviour
{
    [Tooltip("Mouse wheel rolling control lens please enter, the speed of the back")]
    [Range(0.5f, 2f)] public float scrollSpeed = 1f;
    [Tooltip("Right mouse button control lens X axis rotation speed")]
    [Range(0.0f, 2f)] public float rotateXSpeed = 1f;
    [Tooltip("Right mouse button control lens Y axis rotation speed")]
    [Range(0.0f, 2f)] public float rotateYSpeed = 1f;
    [Tooltip("Mouse wheel press, lens translation speed")]
    [Range(0.5f, 2f)] public float moveSpeed = 1f;
    [Tooltip("The keyboard controls how fast the camera moves")]
    [Range(0.5f, 2f)] public float keyMoveSpeed = 1f;

    //Whether the lens control operation is performed
    public bool operate = true;

    public bool Orthographic = false;

    //Whether keyboard control lens operation is performed
    public bool isKeyOperate = true;

    //Whether currently in rotation
    [SerializeField]
    private bool isRotate = false;

    //Is currently in panning
    private bool isMove = false;

    //Camera transform component cache
    private Transform m_transform;

    //The initial position of the camera at the beginning of the operation
    private Vector3 traStart;

    //The initial position of the mouse as the camera begins to operate
    private Vector3 mouseStart;

    //Is the camera facing down
    private bool isDown = false;

    public Camera MyCam;

    // Start is called before the first frame update
    void Start()
    {
        m_transform = transform;
        isExistTarget = false;
        MPXObjectManager.Inst.ChangeTarget.AddListener(ChangeTarget);
    }

    private void ChangeTarget(MPXUnityObject obj)
    {
        targetCollider = obj.MyCol;
        isExistTarget = true;
    }

    bool isExistTarget = false;
    // Update is called once per frame
    void Update()
    {
        if (operate && isExistTarget)
        {
            //When it is in the translation state, and the mouse wheel is released, it will exit the translation state
            if (isMove && Input.GetMouseButtonUp(2))
            {
                isMove = false;
            }

            //Whether it's in a rotational state
            if (!Orthographic && isRotate)
            {
                // simulate the unity editor operation: right click, the keyboard can control the lens movement
                if (isKeyOperate)
                {
                    float speed = keyMoveSpeed;
                    // press LeftShift to make speed *2
                    //按下LeftShift使得速度*2
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        speed = 2f * speed;
                    }
                    // press W on the keyboard to move the camera forward
                    if (Input.GetKey(KeyCode.W))
                    {
                        m_transform.position += m_transform.forward * Time.deltaTime * 10f * speed;
                    }
                    // press the S key on the keyboard to back up the camera
                    if (Input.GetKey(KeyCode.S))
                    {
                        m_transform.position -= m_transform.forward * Time.deltaTime * 10f * speed;
                    }
                    // press A on the keyboard and the camera will turn left
                    if (Input.GetKey(KeyCode.A))
                    {
                        m_transform.position -= m_transform.right * Time.deltaTime * 10f * speed;
                    }
                    // press D on the keyboard to turn the camera to the right
                    if (Input.GetKey(KeyCode.D))
                    {
                        m_transform.position += m_transform.right * Time.deltaTime * 10f * speed;
                    }
                }
            }

            // whether it is in the translation state
            if (isMove)
            {
                // mouse offset on the screen
                Vector3 offset = Input.mousePosition - mouseStart;
                // final position = initial position + offset
                //m_transform.position = traStart + m_transform.up * -offset.y * 0.1f * moveSpeed + m_transform.right * -offset.x * 0.1f * moveSpeed;
            }
            // click the mouse wheel to enter translation mode
            else if (Input.GetMouseButtonDown(2))
            {
                // translation begins
                isMove = true;
                // record the initial position of the mouse
                mouseStart = Input.mousePosition;
                // record the initial position of the camera
                traStart = m_transform.position;
            }
            // how much did the roller roll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            // scroll to scroll or not
            if (scroll != 0)
            {
                m_transform.position += m_transform.forward * scroll * 1000f * Time.deltaTime * scrollSpeed;
                CameraPositionLimit();
                // position = current position + scroll amount
            }
        }
    }


    public Collider[] targetCollider;
    Bounds bounds;
    float angle;
    void CameraPositionLimit()
    {
        //if (bounds.)
        {
            bounds = GetBounds();
        }
        Vector3 objectSizes = bounds.max - bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        objectSize = objectSize * 0.8f;
        angle = Mathf.Clamp(MyCam.transform.position.y, objectSize, objectSize * 3.0f);
        MyCam.transform.position = new Vector3(MyCam.transform.position.x, angle, -angle);
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
}


