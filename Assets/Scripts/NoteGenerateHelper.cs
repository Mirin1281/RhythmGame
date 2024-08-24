using UnityEngine;
using System.Threading;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NoteGenerateHelper : MonoBehaviour
{
    [field: SerializeField] public NormalNotePool NormalNotePool { get; private set; }
    [field: SerializeField] public SlideNotePool SlideNotePool { get; private set; }
    [field: SerializeField] public HoldNotePool HoldNotePool { get; private set; }
    [field: SerializeField] public FlickNotePool FlickNotePool { get; private set; }
    [field: SerializeField] public SkyNotePool SkyNotePool { get; private set; }
    [field: SerializeField] public ArcNotePool ArcNotePool { get; private set; }
    [field: SerializeField] public LinePool LinePool { get; private set; }
    
    [field: SerializeField] public NoteInput NoteInput { get; private set; }
    [field: SerializeField] public Metronome Metronome { get; private set; }

    public CancellationToken Token => destroyCancellationToken;

    /// <summary>
    /// 通常、スライド、フリックのノーツを返します
    /// (それ以外は個別のメソッドを使ってください)
    /// </summary>
    public NoteBase GetNote(NoteType noteType, int index = 0) => noteType switch
    {
        NoteType.Normal => NormalNotePool.GetNote(index),
        NoteType.Slide => SlideNotePool.GetNote(),
        NoteType.Flick => FlickNotePool.GetNote(),
        _ => throw new ArgumentOutOfRangeException()
    };
    public HoldNote GetHold() => HoldNotePool.GetNote();
    public SkyNote GetSky() => SkyNotePool.GetNote();
    public ArcNote GetArc() => ArcNotePool.GetNote();

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