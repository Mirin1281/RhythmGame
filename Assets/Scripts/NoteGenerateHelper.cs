using UnityEngine;
using System.Threading;

public class NoteGenerateHelper : MonoBehaviour
{
    [field: SerializeField] public NormalNotePool NormalNotePool { get; private set; }
    [field: SerializeField] public SlideNotePool SlideNotePool { get; private set; }
    [field: SerializeField] public HoldNotePool HoldNotePool { get; private set; }
    [field: SerializeField] public FlickNotePool FlickNotePool { get; private set; }
    [field: SerializeField] public SkyNotePool SkyNotePool { get; private set; }
    [field: SerializeField] public ArcPool ArcNotePool { get; private set; }
    [field: SerializeField] public LinePool LinePool { get; private set; }
    [field: SerializeField] public NoteInput NoteInput { get; private set; }
    [field: SerializeField] public Metronome Metronome { get; private set; }

    public CancellationToken Token => destroyCancellationToken;
}