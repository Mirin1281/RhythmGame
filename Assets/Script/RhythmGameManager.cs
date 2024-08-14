using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    /// <summary>
    /// 通常は6～15程度を想定
    /// </summary>
    public static float Speed = 12f;
    public static float Offset;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
        
        //DontDestroyOnLoad(this);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetOffset()
    {
        Offset = 0;
    }
}
