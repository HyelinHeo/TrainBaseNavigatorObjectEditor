using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MPXRemote.Message;
using UnityEngine;

public class StartLibrary : MonoBehaviour
{
    const string MAIN_FORM = "TrainBaseNavigator";
    [SerializeField]
    string mainForm;
    Process[] processeList;
    const float DELAY_TIME = 5000f;//단위 밀리세컨

    bool endDelay;

    [SerializeField]
    private bool threadAlive;
    Thread thread;
    private void Awake()
    {
        InitSingleton();
        InitThread();
    }

    void InitThread()
    {
        endDelay = false;
        thread = new Thread(delegate () { MonitoringProcess(); });
        thread.Priority = System.Threading.ThreadPriority.Lowest;
        thread.IsBackground = true;
        isProcessDead = false;
        mainForm = string.Empty;
        thread.Start();
        threadAlive = true;
    }

    private void InitSingleton()
    {
        Message.Inst.Init();
        ReceiverManager.Inst.Init();
        SenderManager.Inst.Init();
        ControlScene.Inst.Init();
        CreateObject.Inst.Init();
        MPXObjectManager.Inst.Init();
    }

    private void Start()
    {
        ReceiverManager.Inst.OnReceiveP.AddListener(OnReceiveProcess);
        ReceiverManager.Inst.StartReceive();
    }

    private void OnReceiveProcess(Protocol p)
    {
        if (p.PType == Protocol.ProtocolType.EventProcess)
        {
            EventProcess ePro = (EventProcess)p;
            mainForm = ePro.ProcessName;
            mainForm = mainForm.Replace(".exe", "");
            endDelay = true;
        }
    }

    void MonitoringProcess()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (threadAlive)
        {
            if (endDelay)
            {
                processeList = Process.GetProcessesByName(mainForm);
                if (processeList.Length < 1 || mainForm == string.Empty)
                {
                    isProcessDead = true;
                    processeList = null;
                }
            }
            else
            {
                if (sw.ElapsedMilliseconds > DELAY_TIME)
                {
                    endDelay = true;
                    sw.Stop();
                }
            }
        }
    }

    bool isProcessDead = false;
    private void Update()
    {
        if (isProcessDead && threadAlive)
        {
            threadAlive = false;
            thread.Abort();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
