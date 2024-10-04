using CriWare;
using NoteGenerating;
using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    // 通常は60～180程度を想定
    static int SpeedBase = 160;
    static readonly int DefaultSpeedBase = 160;
    public static float Speed => SpeedBase / 10f;
    public static float Speed3D => SpeedBase / 2f;

    static int OffsetBase;
    public static float Offset => OffsetBase / 100f;

    public static readonly float DebugBpm = 200;

    static readonly float MasterVolume = 0.6f;
    float bgmVolume = 0.8f;
    public float BGMVolume
    {
        get => MasterVolume * bgmVolume;
        set { bgmVolume = value; }
    }
    public float RawBGMVolume => bgmVolume;
    float seVolume = 0.8f;
    public float SEVolume
    {
        get => MasterVolume * seVolume;
        set { seVolume = value; }
    }
    public float RawSEVolume => seVolume;

    public MusicMasterData MusicMasterData { get; set; }
    public Result Result { get; set; }
    public static Difficulty Difficulty { get; set; }

    public static int SelectedIndex { get; set; }
    

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機させた方が
    /// 扱いが簡単になるため、LPB4でこの回数分待機してから処理を行う
    /// (今後削除する可能性が高い)
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        OffsetBase = 0;
        Application.targetFrameRate = 60;
        SpeedBase = DefaultSpeedBase;
        Difficulty = Difficulty.Hard;
        SelectedIndex = -1;
    }

    void Start()
    {
        //base.Awake();
        // キューデータをスクリプトから流す
        (string sheet, string name)[] cueSheetAndNames = new (string, string)[]
        {
            ("my1", "my1"),
            ("my2", "my2"),
        };

        for(int i = 0; i < cueSheetAndNames.Length; i++)
        {
            var c = cueSheetAndNames[i];
            var sheet = CriAtom.GetCueSheet(c.sheet);
            if(sheet == null)
            {
                CriAtom.AddCueSheet(c.sheet, c.name + ".acb", "");
            }
        }
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
