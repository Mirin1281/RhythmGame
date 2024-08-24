using System.Collections.Generic;
using UnityEngine;

public class HoldNotePool : PoolBase<HoldNote>
{
    public HoldNote GetNote()
    {
        var holdNote = GetInstance();
        holdNote.State =  HoldNote.InputState.Idle;
        holdNote.Grade = NoteGrade.None;
        //holdNote.SetWidth(3f);
        return holdNote;
    }

    public List<HoldNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
