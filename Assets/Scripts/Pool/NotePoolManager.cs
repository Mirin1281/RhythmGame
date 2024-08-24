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

    public NoteBase GetNote(NoteType type, int index = 0) => type switch
    {
        NoteType.Normal => NormalPool.GetNote(index),
        NoteType.Slide => SlidePool.GetNote(),
        NoteType.Flick => FlickPool.GetNote(),
        NoteType.Hold => HoldPool.GetNote(),
        NoteType.Sky => SkyPool.GetNote(),
        NoteType.Arc => ArcPool.GetNote(),
        _ => throw new System.Exception()
    };

    public void SetSimultaneousSprite(NoteBase note, NoteType type)
    {
        if((type is NoteType.Normal or NoteType.Slide or NoteType.Flick) == false) return;
        var sprite = GetSimultaneousSprite(type);
        if(type == NoteType.Normal)
        {
            var normal = note as NormalNote;
            normal.SetSprite(sprite);
        }
        else if(type == NoteType.Slide)
        {
            var slide = note as SlideNote;
            slide.SetSprite(sprite);
        }
        else if(type == NoteType.Flick)
        {
            var flick = note as FlickNote;
            flick.SetSprite(sprite);
        }


        Sprite GetSimultaneousSprite(NoteType type) => type switch
        {
            NoteType.Normal => NormalPool.GetSimultaneousSprite(),
            NoteType.Slide => SlidePool.GetSimultaneousSprite(),
            NoteType.Flick => FlickPool.GetSimultaneousSprite(),
            /*NoteType.Hold => holdNotePool,
            NoteType.Sky => skyNotePool,
            NoteType.Arc => arcNotePool,*/
            _ => throw new System.Exception()
        };
    }
}
