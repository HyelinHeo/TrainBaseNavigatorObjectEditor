using MPXRemote.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Message : Singleton<Message>
{
    public RectTransform Content;
    public Text Txt;
    public Image GUIMessageImag;
    public Image GUIViewportImg;

    /// <summary>
    /// 추후 수정
    /// </summary>
    const int KEY_F1 = 112;
    const int KEY_DELETE = 46;

    private float screenHeight;
    bool isOpen;
    bool isClear;

    public override void Init()
    {
        screenHeight = Screen.height;
        Txt.text = string.Empty;

        GUIMessageVisible(false);
        isOpen = false;
        isClear = false;
        //Debug.Log(Content.sizeDelta);
    }

    public void AddMessage(string msg)
    {
        Txt.text += "\r\n" + msg;
    }

    public void Clear()
    {
        Txt.text = string.Empty;
    }

    public void ShowDebug(Protocol p)
    {
        if (p.PType==Protocol.ProtocolType.EventDebug)
        {
            EventDebug eDebug = (EventDebug)p;

            if (eDebug.Command == EventDebug.COMMAND_VIEW_ONOFF)
            {
                isOpen = !isOpen;
            }
            else if (eDebug.Command == EventDebug.COMMAND_VIEW_CLEAR)
            {
                isClear = true;
            }
            if (isOpen)
            {
                GUIMessageVisible(true);
            }
            else
            {
                GUIMessageVisible(false);
            }
            if (isClear)
            {
                Clear();
                isClear = false;
            }
        }
    }
    /// <summary>
    /// Visible Every Debug Message
    /// </summary>
    /// <param name="show"></param>
    void GUIMessageVisible(bool show)
    {
        GUIMessageImag.enabled = show;
        GUIViewportImg.enabled = show;
        Txt.enabled = show;
        FrameTxt.enabled = show;
    }

    // Attach this to a GUIText to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 frames.

    public float updateInterval = 0.5F;

    float accum = 0; // FPS accumulated over the interval
    int frames = 0; // Frames drawn over the interval
    float timeleft; // Left time for current interval

    public Text FrameTxt;
    void Update()
    {
        if (isOpen)
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (timeleft <= 0.0)
            {
                float fps = accum / frames;
                string format = string.Format("{0:F2} FPS", fps);
                FrameTxt.text = format;

                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}
