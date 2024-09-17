using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    // 通常は60～180程度を想定
    static int SpeedBase = 160;
    public static float Speed => SpeedBase / 10f;
    static readonly int DefaultSpeedBase = 160;
    
    public static float Speed3D => SpeedBase / 2f;

    static int OffsetBase;
    public static float Offset => OffsetBase / 100f;

    public MusicMasterData MusicMasterData { get; set; }
    

    public static readonly float DebugBpm = 200;

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機させた方が
    /// 扱いが簡単になるため、LPB4でこの回数分待機してから処理を行う
    /// (今後削除する可能性が高い)
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        OffsetBase = 0;
        Application.targetFrameRate = 60;
        SpeedBase = DefaultSpeedBase;
    }
    void OnApplicationQuit()
    {
        SpeedBase = DefaultSpeedBase;
    }

    public static void SetSpeed(bool isUp)
    {
        if(isUp)
        {
            SpeedBase++;
        }
        else
        {
            SpeedBase--;
        }
    }
    public static void SetSpeed(int speedBase)
    {
        SpeedBase = speedBase;
    }

    public static void SetOffset(bool isUp)
    {
        if(isUp)
        {
            OffsetBase++;
        }
        else
        {
            OffsetBase--;
        }
    }
    public static void SetOffset(int offsetBase)
    {
        OffsetBase = offsetBase;
    }
}
