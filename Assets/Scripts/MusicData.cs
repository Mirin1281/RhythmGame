using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MusicData",
    menuName = "ScriptableObject/MusicData")
]
public class MusicData : ScriptableObject
{
    [SerializeField] string sheetName;
    
    [SerializeField] string cueName;

    [SerializeField] float bpm;

    [Header("’l‚ğ‘å‚«‚­‚·‚é‚Æƒm[ƒc‚ª‚æ‚è‘‚­—‚¿‚Ü‚·(‚æ‚èLate‚É‚È‚é)")]
    [SerializeField] float offset = 0;

    [SerializeField] int startBeatOffset = 3;

    [SerializeField] BPMChangePoint[] bpmChangePoints;
    
    public string SheetName => sheetName;
    public string CueName => cueName;
    public float Bpm => bpm;
    public float Offset => offset;
    public int StartBeatOffset => startBeatOffset;

    public bool TryGetBPMChangeBeatCount(int index, out int beatCount)
    {
        beatCount = 0;
        if(bpmChangePoints.Length > index)
        {
            beatCount = bpmChangePoints[index].BeatCount + startBeatOffset;
            return true;
        }
        return false;
    }

    public float GetChangeBPM(int index) => bpmChangePoints[index].Bpm;
}

[Serializable]
public class BPMChangePoint
{
    [SerializeField, Min(0)] float bpm;
    [SerializeField, Min(0)] int beatCount;

    public float Bpm => bpm;
    public int BeatCount => beatCount;
}