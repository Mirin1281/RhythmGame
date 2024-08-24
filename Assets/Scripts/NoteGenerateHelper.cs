using UnityEngine;
using System.Threading;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NoteGenerateHelper : MonoBehaviour
{
    [field: SerializeField] public NotePoolManager PoolManager { get; private set; }
    [field: SerializeField] public LinePool LinePool { get; private set; }
    
    [field: SerializeField] public NoteInput NoteInput { get; private set; }
    [field: SerializeField] public Metronome Metronome { get; private set; }

    public CancellationToken Token => destroyCancellationToken;

    public HoldNote GetHold() => PoolManager.HoldPool.GetNote();
    public SkyNote GetSky() => PoolManager.SkyPool.GetNote();
    public ArcNote GetArc() => PoolManager.ArcPool.GetNote();

    public float GetTimeInterval(float lpb, int num = 1)
    {
        if(lpb == 0) return 0;
#if UNITY_EDITOR
        if(EditorApplication.isPlaying == false)
        {
            return 240f / 200 / lpb * num;
        }
#endif
        return 240f / Metronome.Bpm / lpb * num;
    }
}