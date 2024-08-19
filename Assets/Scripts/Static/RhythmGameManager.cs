using System.Collections;
using System.Collections.Generic;
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

    public static readonly float DebugNoteInterval = 200;
    public static readonly string DebugNotePreviewObjName = "Preview2D";

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
