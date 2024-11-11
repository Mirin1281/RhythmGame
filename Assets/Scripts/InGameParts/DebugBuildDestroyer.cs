using System.Collections;
using System.Collections.Generic;
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
            Destroy(o);
        }
        Destroy(gameObject);
    }
#endif
}
