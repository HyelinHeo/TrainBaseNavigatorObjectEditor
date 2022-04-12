using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MPXRemote;
using MPXRemote.Message;
using System;
using UnityEngine.Events;

public class ReceiverManager : Singleton<ReceiverManager>
{
    /// <summary>
    /// stack incomming data by communication
    /// </summary>
    Queue<Protocol> ReceiveQueue = new Queue<Protocol>();
    Queue<Protocol> responseQueue = new Queue<Protocol>();
    Queue<Protocol> ObjectQueue = new Queue<Protocol>();

    public class OnReceiveProtocol : UnityEvent<Protocol> { }
    public OnReceiveProtocol OnReceiveP = new OnReceiveProtocol();

    Receiver receiver = new Receiver();
    [SerializeField]
    Protocol protocol;

    bool isOpen;
    public override void Init()
    {
        base.Init();
        ControlScene.Inst.OpenedScene.AddListener(SceneOpend);
        ControlScene.Inst.ClosedScene.AddListener(SceneClosed);
        isOpen = false;
    }

    public void StartReceive()
    {
        receiver.Init("sub1");
        receiver.Start();
        receiver.OnReceive += Receiver_OnReceive;
        Debug.Log("receive init");
    }

    private void SceneOpend()
    {
        isOpen = true;
    }

    private void SceneClosed()
    {
        isOpen = false;
    }

    private void Update()
    {
        Protocol receive = Dequeue(ReceiveQueue);
        if (receive != null)
        {
            Message.Inst.AddMessage("receive : " + receive.ToString());
            CheckProtocolType(receive);
        }
        Protocol response = Dequeue(responseQueue);
        if (response != null)
        {
            Message.Inst.AddMessage("receive : " + response.ToString());
        }
        ///씬이 생성되었을 때만 오브젝트 생성하도록
        if (isOpen)
        {
            Protocol receiveObj = Dequeue(ObjectQueue);
            if (receiveObj != null)
                CreateEventLibraryObject(receiveObj);
        }
    }

    private void Receiver_OnReceive(Protocol p)
    {
        if (p.Request == SenderManager.REQUEST)
        {
            Debug.Log(p);
            SenderManager.Inst.AddSendProtocol(p);
            Enqueue(ReceiveQueue, p);
        }
        else
        {
            Enqueue(responseQueue, p);
        }
    }

    private void CreateEventLibraryObject(Protocol p)
    {
        OnReceiveP.Invoke(p);
    }

    /// <summary>
    /// protocoltype 별로 이벤트 발생
    /// </summary>
    /// <param name="type"></param>
    public void CheckProtocolType(Protocol p)
    {

        switch (p.PType)
        {
            case Protocol.ProtocolType.EventScene:
                OnReceiveP.Invoke(p);
                break;
            case Protocol.ProtocolType.EventScreen:
                SenderManager.Inst.EndProcess(p.ID);
                break;
            case Protocol.ProtocolType.EventSelect:
                SenderManager.Inst.EndProcess(p.ID);
                break;
            case Protocol.ProtocolType.EventInput:
                SenderManager.Inst.EndProcess(p.ID);
                break;
            case Protocol.ProtocolType.EventCreateLibaryObject:
                if (isOpen)
                    CreateEventLibraryObject(p);
                else
                    Enqueue(ObjectQueue, p);
                break;
            case Protocol.ProtocolType.EventProcess:
                OnReceiveP.Invoke(p);
                SenderManager.Inst.EndProcess(p.ID);
                break;
            case Protocol.ProtocolType.EventDebug:
                Message.Inst.ShowDebug(p);
                SenderManager.Inst.EndProcess(p.ID);
                break;
            default:
                SenderManager.Inst.EndProcess(p.ID);
                break;
        }
    }

    /// <summary>
    /// bring the protocol at queue
    /// </summary>
    /// <returns></returns>
    public Protocol Dequeue(Queue<Protocol> receiveP)
    {
        if (receiveP != null && receiveP.Count > 0)
        {
            return receiveP.Dequeue();
        }
        return null;
    }

    /// <summary>
    /// add protocol at queue
    /// </summary>
    /// <param name="p"></param>
    public void Enqueue(Queue<Protocol> receiveP, Protocol p)
    {
        if (receiveP == null)
        {
            receiveP = new Queue<Protocol>();
        }
        receiveP.Enqueue(p);
    }

    void Clear(Queue<Protocol> queue)
    {
        if (queue != null)
        {
            queue.Clear();
            queue = null;
        }
    }

    private void OnDestroy()
    {
        OnDestroyMain();
    }

    void OnDestroyMain()
    {
        //receiver.OnReceive -= Receiver_OnReceive;
        receiver.Dispose();
        //Clear();
    }

    public void Clear()
    {
        Clear(ReceiveQueue);
        Clear(ObjectQueue);
        Clear(responseQueue);
    }
}
