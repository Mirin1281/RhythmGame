using UnityEngine;
using System.Threading;

public class NoteGenerateHelper : MonoBehaviour
{
    [field: SerializeField] public NormalNotePool NormalNotePool { get; set; }
    [field: SerializeField] public SlideNotePool SlideNotePool { get; set; }
    [field: SerializeField] public HoldNotePool HoldNotePool { get; set; }
    [field: SerializeField] public FlickNotePool FlickNotePool { get; set; }
    [field: SerializeField] public ArcPool ArcNotePool { get; set; }
    [field: SerializeField] public NoteInput NoteInput { get; set; }
    [field: SerializeField] public Metronome Metronome { get; set; }

    public CancellationToken Token => destroyCancellationToken;
}