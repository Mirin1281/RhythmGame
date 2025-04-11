using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    static readonly float MasterVolume = 1.0f; // 音量調整用(不変)
    public static readonly float DefaultSpeed = 14f; // 基準のノーツスピード(不変)

    // スクリプト上で変更可能なノーツスピード
    public static float SpeedBase = 1f;

    public static AssetReference FumenReference { get; set; }
    public static string FumenName { get; set; }
    public Result Result { get; set; }

    static GameSetting setting = new();
    public static GameSetting Setting => setting;


    static GameStatus status;
    static GameStatus Status
    {
        get
        {
            return status ??= new GameStatus();
        }
        set
        {
            status = value;
        }
    }

    public static float GetBGMVolume() => MasterVolume * setting.BGMVolume * 0.6f;
    public static float GetSEVolume() => MasterVolume * setting.SEVolume;
    public static float GetNoteVolume() => MasterVolume * setting.NoteSEVolume;


    public static float Speed => SpeedBase * setting.Speed / 5f;
    public static float Offset => setting.Offset / 1000f;


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
        Application.targetFrameRate = 60;
        FumenReference = null;

        if (useJsonData)
        {
            var gameData = SaveLoadUtility.GetDataImmediately<GameData>(ConstContainer.GameDataName);
            gameData ??= new GameData();
            setting = gameData.Setting ?? new GameSetting();
            Status = gameData.Status ?? new GameStatus();
        }
        else
        {
            setting = new GameSetting();
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
        SEManager.Instance.SetCategoryVolume(ConstContainer.SECategory, GetSEVolume());
        SEManager.Instance.SetCategoryVolume(ConstContainer.NoteSECategory, GetNoteVolume());
    }

    void OnApplicationPause(bool goBack)
    {
        if (goBack)
        {
            OnApplicationQuit();
        }
    }

    // OnApplicationQuitはタスクキルすると呼ばれないっぽい
    void OnApplicationQuit()
    {
        if (useJsonData)
        {
            var gameData = new GameData
            {
                Setting = setting,
                Status = Status
            };
            SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
        }
    }

    public void ResetData()
    {
        var gameData = new GameData();
        setting = gameData.Setting ?? new GameSetting();
        Status = gameData.Status ?? new GameStatus();
        SaveLoadUtility.SetDataImmediately(gameData, ConstContainer.GameDataName);
    }
}
