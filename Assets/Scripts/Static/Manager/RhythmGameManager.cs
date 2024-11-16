using CriWare;
using UnityEngine;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    // 不変の音量
    static readonly float MasterVolume = 0.8f;
    
    // スクリプト上で変更可能なノーツスピード
    public static float SpeedBase = 1f;

    public static string FumenAddress { get; set; }
    public Result Result { get; set; }

    static GameSetting Setting { get; set; }
    static GameStatus Status { get; set; }


    public static float SettingBGMVolume
    {
        get => Setting.BGMVolume;
        set => Setting.BGMVolume = value;
    }
    public static float SettingSEVolume
    {
        get => Setting.SEVolume;
        set => Setting.SEVolume = value;
    }
    public static float SettingNoteSEVolume
    {
        get => Setting.NoteSEVolume;
        set => Setting.NoteSEVolume = value;
    }
    public static bool SettingIsNoteMute
    {
        get => Setting.IsNoteMute;
        set => Setting.IsNoteMute = value;
    }
    public static float GetBGMVolume() => MasterVolume * SettingBGMVolume * 0.6f;
    public static float GetSEVolume() => MasterVolume * SettingSEVolume;
    public static float GetNoteVolume() => MasterVolume * SettingNoteSEVolume;    


    // 50~100程度を想定。ゲーム内では"7.0"のような表記で扱う
    public static int SettingSpeed
    {
        get => Setting.Speed;
        set => Setting.Speed = value;
    }
    public static int SettingSpeed3D
    {
        get => Setting.Speed3D;
        set => Setting.Speed3D = value;
    }
    public static float Speed => SpeedBase * SettingSpeed / 5f;
    public static float Speed3D => SpeedBase * SettingSpeed; // 

    // -100~100程度を想定。ゲーム内では"0.000"のような表記で扱う
    public static int SettingOffset
    {
        get => Setting.Offset;
        set => Setting.Offset = value;
    }
    public static float Offset => SettingOffset / 1000f;

    public static bool SettingIsMirror
    {
        get => Setting.IsMirror;
        set => Setting.IsMirror = value;
    }

    
    public static Difficulty Difficulty
    {
        get => Status.Difficulty;
        set => Status.Difficulty = value;
    }
    public static int SelectedIndex
    {
        get => Status.SelectedIndex;
        set => Status.SelectedIndex = value;
    }

#if UNITY_EDITOR
    /// <summary>
    /// falseにするとデフォルトの設定が使用されます
    /// </summary>
    static readonly bool useJsonData = false;
#else
    static readonly bool useJsonData = true;
#endif

    /// <summary>
    /// カメラ制御の際など、ノーツの生成と異なり即座に影響を与えるコマンドは待機
    /// させた方が扱いが簡単になるため、LPB=4でこの回数分待機してから処理を行う
    /// </summary>
    public static readonly int DefaultWaitOnAction = 6;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        SpeedBase = 1f;
        Application.targetFrameRate = 60;
        FumenAddress = null;
        
        if(useJsonData)
        {
            var gameData = SaveLoadUtility.GetDataImmediately<GameData>(ConstContainer.GameDataName);
            Setting = gameData.Setting ?? new GameSetting();
            Status = gameData.Status ?? new GameStatus();
        }
        else
        {
            Setting = new GameSetting();
            Status = new GameStatus();
        }
    }

    // AwakeだとCriWare側でエラーを吐く 許さない
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
                Setting = Setting,
                Status = Status
            };
            SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
        }
    }
}
