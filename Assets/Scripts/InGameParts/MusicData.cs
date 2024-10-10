using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Music",
    menuName = "ScriptableObject/Music",
    order = 0)
]
public class MusicData : ScriptableObject
{
    [SerializeField] string musicName;

    [SerializeField] string internalMusicName;

    [SerializeField] string composerName;

    [SerializeField] string sheetName;
    
    [SerializeField] string cueName;

    [Header("値を大きくするとノーツがより早く落ちます(よりLateになる)")]
    [SerializeField] float offset = 0;

    [SerializeField] int startBeatOffset = 3;

    [SerializeField] float bpm;
    [Space(20)]
    [SerializeField] float previewStart;

    [SerializeField] float previewEnd = 1000;
    [Space(10)]
    [SerializeField] BPMChangePoint[] bpmChangePoints;
    
    public string MusicName => musicName;
    public string InternalMusicName => internalMusicName ?? musicName;
    public string ComposerName => composerName;
    public string SheetName => sheetName;
    public string CueName => cueName;
    public float PreviewStart => previewStart;
    public float PreviewEnd => previewEnd;
    public float Bpm => bpm;
    public float Offset => offset;
    public int StartBeatOffset => startBeatOffset;

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
}

[Serializable]
public struct BPMChangePoint
{
    [SerializeField, Min(0)] float bpm;
    [SerializeField, Min(0)] int beatCount;
    public readonly float Bpm => bpm;
    public readonly int BeatCount => beatCount;
}