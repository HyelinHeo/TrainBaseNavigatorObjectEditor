using MPXRemote.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlScene : Singleton<ControlScene>
{
    public UnityEvent OpenedScene = new UnityEvent();
    public UnityEvent ClosedScene = new UnityEvent();
    public UnityEvent OnCompleteCreateCamera = new UnityEvent();

    public bool IsNew = true;
    public Image FadeImage;

    const string EDITOR_SCENE = "EditorView";
    const string START_SCENE = "Main";

    public override void Init()
    {
        base.Init();
        ReceiverManager.Inst.OnReceiveP.AddListener(OnReceive);
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnReceive(Protocol p)
    {
        if (p.PType==Protocol.ProtocolType.EventScene)
        {
            EventScene eventInfo = (EventScene)p;
            FadeImage.enabled = true;
            if (eventInfo.Command == EventScene.COMMAND_NEW)
            {
                IsNew = true;
                StartCoroutine(AddEditView(eventInfo));
            }
            else if (eventInfo.Command == EventScene.COMMAND_OPEN)
            {
                IsNew = false;//새로 생성한 오브젝트 들이 모두 만들어 졌다면 isnew를 다시 true로 _추후수정20201219
                StartCoroutine(AddEditView(eventInfo));
            }
            else if (eventInfo.Command == EventScene.COMMAND_CLOSE)
            {
                StartCoroutine(RemoveEditView(eventInfo));
            }
        }
    }

    IEnumerator AddEditView(EventScene eventInfo)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(EDITOR_SCENE, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(EDITOR_SCENE));
        //오브젝트 생성(불러올)
        OpenedScene.Invoke();

        SenderManager.Inst.EndProcess(eventInfo.ID);
        //yield return Fade.StartCoroutine(Fade.FadeOut());
    }

    IEnumerator RemoveEditView(EventScene eventInfo)
    {
        ReceiverManager.Inst.Clear();
        CreateObject.Inst.SetObjectList(null);
        CreateObject.Inst.currentObject = null;
        MPXObjectManager.Inst.RotateValue = Vector3.zero;

        if (SenderManager.Inst.Count() > 0)
            SenderManager.Inst.RemoveExceptLastValue();
        if (SceneManager.GetActiveScene().name == EDITOR_SCENE)
        {
            //yield return Fade.StartCoroutine(Fade.FadeIn());
            MPXObjectManager.Inst.Clear();
            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(EDITOR_SCENE);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(START_SCENE));
            ClosedScene.Invoke();
        }
        SenderManager.Inst.EndProcess(eventInfo.ID);
    }

    private void OnDestroy()
    {
        ReceiverManager.Inst.OnReceiveP.RemoveListener(OnReceive);
    }
}
