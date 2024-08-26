using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePoolManager : MonoBehaviour
{
    [field: SerializeField] public NormalNotePool NormalPool { get; private set; }
    [field: SerializeField] public SlideNotePool SlidePool { get; private set; }
    [field: SerializeField] public FlickNotePool FlickPool { get; private set; }
    [field: SerializeField] public HoldNotePool HoldPool { get; private set; }
    [field: SerializeField] public SkyNotePool SkyPool { get; private set; }
    [field: SerializeField] public ArcNotePool ArcPool { get; private set; }

    public NoteBase_2D GetNote2D(NoteType type, int index = 0) => type switch
    {
        NoteType.Normal => NormalPool.GetNote(index),
        NoteType.Slide => SlidePool.GetNote(),
        NoteType.Flick => FlickPool.GetNote(),
        _ => throw new System.Exception()
    };

    public NoteBase GetNote(NoteType type, int index = 0) => type switch
    {
        /*NoteType.Normal => NormalPool.GetNote(index),
        NoteType.Slide => SlidePool.GetNote(),
        NoteType.Flick => FlickPool.GetNote(),*/
        NoteType.Hold => HoldPool.GetNote(),
        NoteType.Sky => SkyPool.GetNote(),
        NoteType.Arc => ArcPool.GetNote(),
        _ => throw new System.Exception()
    };

    public void SetSimultaneousSprite(NoteBase_2D note)
    {
        if(note == null) return;
        var sprite = GetSimultaneousSprite(note.Type);
        if(sprite == null) return;
        note.SetSprite(sprite);


        Sprite GetSimultaneousSprite(NoteType type) => type switch
        {
            NoteType.Normal => NormalPool.GetSimultaneousSprite(),
            NoteType.Slide => SlidePool.GetSimultaneousSprite(),
            NoteType.Flick => FlickPool.GetSimultaneousSprite(),
            NoteType.Hold => null,
            /*NoteType.Hold => holdNotePool,
            NoteType.Sky => skyNotePool,
            NoteType.Arc => arcNotePool,*/
            _ => throw new System.Exception()
        };
    }
}
