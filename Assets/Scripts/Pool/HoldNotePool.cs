using System.Collections.Generic;
using UnityEngine;

public class HoldNotePool : PoolBase<HoldNote>
{
    public HoldNote GetNote()
    {
        var holdNote = GetInstance();
        holdNote.State =  HoldNote.InputState.Idle;
        holdNote.SetWidth(1f);
        holdNote.SetAlpha(1f);
        return holdNote;
    }

    public List<HoldNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
