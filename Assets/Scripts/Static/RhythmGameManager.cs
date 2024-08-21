using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NoteGenerating;
using UnityEditor;
using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    /// <summary>
    /// 通常は6～15程度を想定
    /// </summary>
    public static float Speed = 16f;
    public static float Speed3D = 80f;
    public static float Offset;

    public static readonly float DebugBpm = 200;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
        Speed = 16f;
        Speed3D = 80f;
        
        //DontDestroyOnLoad(this);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetOffset()
    {
        Offset = 0;
    }

    void OnApplicationQuit()
    {
        Speed = 16f;
        Speed3D = 80f;
    }
}
