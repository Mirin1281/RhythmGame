using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNotePool : PoolBase<SlideNote>
{
    public SlideNote GetNote()
    {
        var slideNote = GetInstance();
        slideNote.SetRotate(0);
        slideNote.SetWidth(3f);
        return slideNote;
    }

    public List<SlideNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
