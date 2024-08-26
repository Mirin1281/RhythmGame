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
    public static float Speed { get; private set; } = 16f;
    public static float Speed3D { get; private set; }  = 80f;
    static readonly float DefaultSpeed = 16f;
    static readonly float DefaultSpeed3D = 80f;
    
    public static float Offset;

    public static readonly float DebugBpm = 200;

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機させた方が
    /// 扱いが簡単になるため、LPB4でこの回数分待機してから処理を行う
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

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
        Speed = DefaultSpeed;
        Speed3D = DefaultSpeed3D;
    }

    /*public static void SetSpeed(float rate = -1)
    {
        if(rate == -1)
        {
            Speed = DefaultSpeed;
            Speed3D = DefaultSpeed3D;
        }
        else
        {
            Speed = DefaultSpeed * rate;
            Speed3D = DefaultSpeed3D * rate;
        }
    }*/
}
