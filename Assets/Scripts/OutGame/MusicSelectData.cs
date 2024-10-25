using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(
    fileName = "M_",
    menuName = "ScriptableObject/MusicSelect",
    order = 0)
]
public class MusicSelectData : ScriptableObject
{
    [field: SerializeField] public string MusicName { get; private set; }
    [field: SerializeField] public string ComposerName { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public string SheetName { get; private set; }
    [field: SerializeField] public float PreviewStartTime { get; private set; }
    [field: SerializeField] public float PreviewEndTime { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public Sprite Illust { get; private set; }
    [field: SerializeField] public string IllustratorName { get; private set; }

    [field: Space(10)]
    //[field: SerializeField] public AssetReference NormalFumenReference { get; private set; }
    [field: SerializeField] public string NormalFumenAddress { get; private set; }
    [SerializeField] int level_normal = -1;
    public int Level_normal => level_normal;
    [field: SerializeField] public string HardFumenAddress { get; private set; }
    [SerializeField] int level_hard = -1;
    public int Level_hard => level_hard;
    [field: SerializeField] public string ExtraFumenAddress { get; private set; }
    [SerializeField] int level_extra = -1;
    public int Level_extra => level_extra;

    [field: Space(10)]
    [field: SerializeField] public float Bpm { get; private set; }
    [field: SerializeField, Tooltip("大きくするとLateになります")] public float Offset { get; private set; }
    [field: SerializeField] public int StartBeatOffset { get; private set; }

    [Space(10)]
    [SerializeField] BPMChangePoint[] bpmChangePoints;

    [Serializable]
    struct BPMChangePoint
    {
        [SerializeField, Min(0)] float bpm;
        [SerializeField, Min(0)] int beatCount;
        public readonly float Bpm => bpm;
        public readonly int BeatCount => beatCount;
    }

    /// <summary>
    /// 譜面データをロードする際のアドレスを取得します。引数を省略するとマネージャーの情報を参照します
    /// </summary>
    public string GetFumenAddress(Difficulty difficulty = Difficulty.None)
    {
        if(difficulty == Difficulty.None)
        {
            difficulty = RhythmGameManager.Difficulty;
        }
        return difficulty switch
        {
            Difficulty.Normal => NormalFumenAddress,
            Difficulty.Hard => HardFumenAddress,
            Difficulty.Extra => ExtraFumenAddress,
            _ => throw new System.Exception()
        };
    }

    /// <summary>
    /// 譜面データの難易度を取得します。引数を省略するとマネージャーの情報を参照します
    /// </summary>
    public int GetFumenLevel(Difficulty difficulty = Difficulty.None)
    {
        if(difficulty == Difficulty.None)
        {
            difficulty = RhythmGameManager.Difficulty;
        }
        return difficulty switch
        {
            Difficulty.Normal => Level_normal,
            Difficulty.Hard => Level_hard,
            Difficulty.Extra => Level_extra,
            _ => throw new System.Exception()
        };
    }

    /// <summary>
    /// BPMの変化点のカウントを取得します
    /// </summary>
    public bool TryGetBPMChangeBeatCount(int index, out int beatCount)
    {
        beatCount = 0;
        if(bpmChangePoints.Length > index)
        {
            beatCount = bpmChangePoints[index].BeatCount;
            return true;
        }
        return false;
    }

    public float GetChangeBPM(int index) => bpmChangePoints[index].Bpm;

#if UNITY_EDITOR
    public void SetFumenAddress(string name, Difficulty difficulty)
    {
        if(difficulty == Difficulty.Normal)
        {
            NormalFumenAddress = name;
        }
        else if(difficulty == Difficulty.Hard)
        {
            HardFumenAddress = name;
        }
        else if(difficulty == Difficulty.Extra)
        {
            ExtraFumenAddress = name;
        }
    }
#endif
}

public enum Difficulty { None, Normal, Hard, Extra }