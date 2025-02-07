using UnityEngine;

public class DebugBuildDestroyer : MonoBehaviour
{
    [SerializeField] GameObject[] objs;
#if UNITY_EDITOR
#else
    void Awake()
    {
        foreach(var o in objs)
        {
            if(o != null) Destroy(o);
        }
        Destroy(gameObject);
    }
#endif
}
