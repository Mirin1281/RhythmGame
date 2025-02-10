using CriWare;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    // 不変の音量
    static readonly float MasterVolume = 0.8f;
    public static readonly float DefauleSpeed = 14f;

    // スクリプト上で変更可能なノーツスピード
    public static float SpeedBase = 1f;

    public static AssetReference FumenReference { get; set; }
    public static string FumenName { get; set; }
    public Result Result { get; set; }

    static GameSetting Setting { get; set; } = new();
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
    public static float Speed => SpeedBase * SettingSpeed / 5f;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        SpeedBase = 1f;
        Application.targetFrameRate = 60;
        FumenReference = null;
        Application.quitting += SaveData;

        if (useJsonData)
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

    // AwakeだとCriWare側でエラーを吐く
    void Start()
    {
        var sheet = CriAtom.GetCueSheet("SESheet");
        if (sheet == null)
        {
            CriAtom.AddCueSheet("SESheet", "SESheet.acb", "");
        }
    }

    // OnApplicationQuitはタスクキルすると呼ばれないっぽい
    static void SaveData()
    {
        if (useJsonData)
        {
            var gameData = new GameData
            {
                Setting = Setting,
                Status = Status
            };
            SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
        }
    }

    public void ResetData()
    {
        var gameData = new GameData();
        Setting = gameData.Setting ?? new GameSetting();
        Status = gameData.Status ?? new GameStatus();
        SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
    }
}
