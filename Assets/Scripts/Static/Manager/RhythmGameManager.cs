using CriWare;
using Cysharp.Threading.Tasks;
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

    /// <summary>
    /// エディタ上での仮のBPM
    /// </summary>
    public static readonly float DebugBpm = 200;

    static readonly float MasterVolume = 0.6f;
    static float bgmVolume = 0.8f;
    public float BGMVolume
    {
        get => 0.6f * MasterVolume * bgmVolume;
        set { bgmVolume = value; }
    }
    public float RawBGMVolume => bgmVolume;
    static float seVolume = 0.8f;
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
    /// Falseにするとデフォルトの設定が使用されます
    /// </summary>
    static readonly bool useJsonData = false;
    

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機させた方が
    /// 扱いが簡単になるため、LPB4でこの回数分待機してから処理を行う
    /// (今後削除する可能性が高い)
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        Application.targetFrameRate = 60;
        
        if(useJsonData)
        {
            var gameData = SaveLoadUtility.GetDataImmediately<GameData>(ConstContainer.GameDataName);
            gameData ??= new GameData();
            bgmVolume = gameData.BgmVolume;
            seVolume = gameData.SeVolume;
            SpeedBase = Mathf.RoundToInt(gameData.Speed * 10f);
            OffsetBase = Mathf.RoundToInt(gameData.Offset * 100f);
            Difficulty = gameData.Difficulty;
            SelectedIndex = gameData.SelectedIndex;
        }
        else
        {
            bgmVolume = 0.8f;
            seVolume = 0.8f;
            SpeedBase = DefaultSpeedBase;
            OffsetBase = 0;
            Difficulty = Difficulty.Hard;
            SelectedIndex = -1;
        }
    }

    // AwakeだとCriWare側でエラーを吐く
    void Start()
    {
        // キューデータをスクリプトから流す
        (string sheet, string name)[] cueSheetAndNames = new (string, string)[]
        {
            ("my1", "my1"),
            ("my2", "my2"),
            ("start_freeze", "start_freeze"),
            ("ti", "ti"),
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
        if(useJsonData)
        {
            var gameData = new GameData
            {
                BgmVolume = bgmVolume,
                SeVolume = seVolume,
                Speed = Speed,
                Offset = Offset,
                Difficulty = Difficulty,
                SelectedIndex = SelectedIndex,
            };
            SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
        }
        else
        {
            SpeedBase = DefaultSpeedBase;
        }
    }

    public static void SetSpeed(bool isAdd)
    {
        if(isAdd)
        {
            SpeedBase++;
        }
        else
        {
            SpeedBase--;
        }
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
}
