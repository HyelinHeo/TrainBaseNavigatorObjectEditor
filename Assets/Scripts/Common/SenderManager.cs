using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPXRemote;
using MPXRemote.Message;
using System;

public class SenderManager : Singleton<SenderManager>
{
    Sender sender = new Sender();

    public const int REQUEST = 0;
    public const int RESPONSE = 1;

    int protocolID;

    List<Protocol> processList = new List<Protocol>();

    public override void Init()
    {
        base.Init();
        sender.Init("sub2");
        sender.Start();
        sender.Sended += Sender_Sended;
        protocolID = 0;
    }

    private void Sender_Sended(Protocol p)
    {
        Debug.Log("Send : " + p.ToString());
    }

    /// <summary>
    /// sub로 전송
    /// </summary>
    /// <param name="p"></param>
    void Send(Protocol p)
    {
        p.MyTime = DateTime.Now;
        sender.Send(p);
        Message.Inst.AddMessage("Send : " + p.ToString());
    }

    public void SendRequest(Protocol p)
    {
        p.ID = protocolID++;
        p.Request = REQUEST;
        Send(p);
    }

    /// <summary>
    /// 전송할 프로토콜 추가
    /// </summary>
    /// <param name="p"></param>
    public void AddSendProtocol(Protocol p)
    {
        processList.Add(p);
    }

    Protocol FindProcess(int protocolId)
    {
        if (processList != null && processList.Count > 0)
        {
            return processList.Find(o => o.ID == protocolId);
        }
        return null;
    }

    public int Count()
    {
        if (processList != null)
        {
            if (processList.Count > 0)
                return processList.Count;
            else
                return 0;
        }
        else
            return -1;
    }

    public void RemoveExceptLastValue()
    {
        if (processList != null)
        {
            if (Count() > 1)
            {
                processList.RemoveRange(0, Count() - 1);
            }
        }
    }

    public void CreateList()
    {
        if (processList == null)
        {
            processList = new List<Protocol>();
        }
    }

    public void EndProcess(int protocolId)
    {
        Protocol p = FindProcess(protocolId);
        if (p != null)
        {
            p.Request = Protocol.TYPE_RESPONSE;
        }
    }

    public void EndErrorProcess(int protocolId)
    {
        Protocol p = FindProcess(protocolId);
        if (p != null)
        {
            p.Request = Protocol.TYPE_RESPONSE_ERROR;
        }
    }

    private void Update()
    {
        if (processList != null && processList.Count > 0)
        {
            if (processList[0].Request == Protocol.TYPE_RESPONSE|| processList[0].Request==Protocol.TYPE_RESPONSE_ERROR)
            {
                Send(processList[0]);
                processList.RemoveAt(0);
            }
        }
    }

    private void OnDestroy()
    {
        if (sender != null)
        {
            sender.Dispose();
            sender.Sended -= Sender_Sended;
        }
        Clear();
        processList = null;
    }

    public void Clear()
    {
        if (processList != null)
        {
            processList.Clear();
        }
    }

    public void ProcessStart()
    {
        sender.Start();
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }
}
