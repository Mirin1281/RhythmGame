using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;

public class RhythmGameManager : SingletonMonoBehaviour<RhythmGameManager>
{
    static readonly float MasterVolume = 1.5f; // 不変の音量
    public static readonly float DefaultSpeed = 14f; // 不変のノーツスピード

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


    // setting.Speedは50~100程度を想定。ゲーム内では"7.0"のような表記で扱う
    public static float Speed => SpeedBase * setting.Speed / 5f;

    // setting.Offsetは-100~100程度を想定。ゲーム内では"0.000"のような表記で扱う
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
        Application.quitting += OnQuit;

        if (useJsonData)
        {
            var gameData = SaveLoadUtility.GetDataImmediately<GameData>(ConstContainer.GameDataName);
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
    }

    // OnApplicationQuitはタスクキルすると呼ばれないっぽい
    static void OnQuit()
    {
        SpeedBase = 1f;
        SetDarkModeAsync(false, true).Forget();

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

    public static async UniTask SetDarkModeAsync(bool enabled, bool immediate = false)
    {
        await UniTask.CompletedTask;
        setting.IsDark = enabled;
        FadeLoadSceneManager.Instance.FadeColor = enabled ? Color.black : Color.white;

        /*Material invertMat;
        Material negativeMat;
        if (immediate)
        {
            invertMat = Addressables.LoadAssetAsync<Material>(ConstContainer.InvertColorMaterialPath).WaitForCompletion();
            negativeMat = Addressables.LoadAssetAsync<Material>(ConstContainer.NegativeMaterialPath).WaitForCompletion();
        }
        else
        {
            invertMat = await Addressables.LoadAssetAsync<Material>(ConstContainer.InvertColorMaterialPath);
            negativeMat = await Addressables.LoadAssetAsync<Material>(ConstContainer.NegativeMaterialPath);
        }
        float value = enabled ? 1 : 0;
        invertMat.SetFloat("_Value", value);
        negativeMat.SetFloat("_BlendRate", value);
        Addressables.Release(invertMat);
        Addressables.Release(negativeMat);*/
    }
}
