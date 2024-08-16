using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNotePool : PoolBase<SlideNote>
{
    public SlideNote GetNote()
    {
        var slideNote = GetInstance();
        slideNote.SetWidth(3f);
        return slideNote;
    }
}
