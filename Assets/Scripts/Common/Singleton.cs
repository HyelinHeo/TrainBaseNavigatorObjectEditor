using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T g_Instance = default(T);
    public static T Inst
    {
        get
        {
            if (g_Instance == null)
            {
                T t = GameObject.FindObjectOfType(typeof(T)) as T;
                if (t == null)
                {
                    string strName = typeof(T).ToString();
                    GameObject go = new GameObject(string.Format("[{0}]", strName));
                    g_Instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    g_Instance = t;
                }
            }

            return g_Instance;
        }
    }

    private void OnDestroy()
    {
        if (g_Instance != null)
            Destroy(g_Instance);

        g_Instance = null;
    }

    public virtual void Init() { }
}
