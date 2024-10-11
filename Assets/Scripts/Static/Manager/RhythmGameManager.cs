using CriWare;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    static readonly float MasterVolume = 0.6f;
    public static float SettingBGMVolume { get; set; } = 0.8f;
    public static float SettingSEVolume { get; set; } = 0.8f;
    public static float SettingNoteVolume { get; set; } = 0.8f;
    public static float GetBGMVolume() => MasterVolume * SettingBGMVolume * 0.6f;
    public static float GetSEVolume() => MasterVolume * SettingSEVolume;
    public static float GetNoteVolume() => MasterVolume * SettingNoteVolume;

    public static float SpeedBase = 1f;

    // 50~100程度を想定。ゲーム内では"7.0"のような表記で扱う
    public static int SettingSpeed { get; set; } = 70;
    public static int SettingSpeed3D { get; set; } = 70;
    public static float Speed => SpeedBase * SettingSpeed / 5f;
    public static float Speed3D => SpeedBase * SettingSpeed3D;

    // -100~100程度を想定。ゲーム内では"0.000"のような表記で扱う
    public static int SettingOffset { get; set; }
    public static float Offset => SettingOffset / 1000f;

    public static bool SettingIsMirror { get; set; }

    public MusicMasterData MusicMasterData { get; set; }
    public Result Result { get; set; }
    public static Difficulty Difficulty { get; set; } = Difficulty.Normal;
    public static int SelectedIndex { get; set; } = -1;

#if UNITY_EDITOR
    /// <summary>
    /// falseにするとデフォルトの設定が使用されます
    /// </summary>
    static readonly bool useJsonData = false;
#else
    static readonly bool useJsonData = true;
#endif

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機させた方が
    /// 扱いが簡単になるため、LPB4でこの回数分待機してから処理を行う
    /// (今後削除する可能性が高い)
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        SpeedBase = 1f;
        Application.targetFrameRate = 60;
        
        if(useJsonData)
        {
            var gameData = SaveLoadUtility.GetDataImmediately<GameData>(ConstContainer.GameDataName);
            gameData ??= new GameData();
            SettingBGMVolume = gameData.BGMVolume;
            SettingSEVolume = gameData.SEVolume;
            SettingNoteVolume = gameData.NoteVolume;

            SettingSpeed = gameData.Speed;
            SettingSpeed3D = gameData.Speed3D;
            SettingOffset = gameData.Offset;

            SettingIsMirror = gameData.IsMirror;

            Difficulty = gameData.Difficulty;
            SelectedIndex = gameData.SelectedIndex;
        }
        else
        {
            SettingBGMVolume = 0.8f;
            SettingSEVolume = 0.8f;
            SettingNoteVolume = 0.8f;
            
            SettingSpeed = 70;
            SettingSpeed3D = 70;
            SettingOffset = 0;

            SettingIsMirror = false;

            Difficulty = Difficulty.Normal;
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

    // OnApplicationQuitはタスクキルすると呼ばれないっぽい
    void OnApplicationPause(bool goBack)
    {
        if (goBack)
        {
            OnApplicationQuit();
        }
    }

    void OnApplicationQuit()
    {
        if(useJsonData)
        {
            var gameData = new GameData
            {
                BGMVolume = SettingBGMVolume,
                SEVolume = SettingSEVolume,
                NoteVolume = SettingNoteVolume,
                Speed = SettingSpeed,
                Speed3D = SettingSpeed3D,
                Offset = SettingOffset,
                IsMirror = SettingIsMirror,
                Difficulty = Difficulty == Difficulty.None ? Difficulty.Normal : Difficulty,
                SelectedIndex = SelectedIndex,
            };
            SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
        }
    }
}
