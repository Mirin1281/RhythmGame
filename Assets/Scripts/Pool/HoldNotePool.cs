using UnityEngine;

public class HoldNotePool : PoolBase<HoldNote>
{
    public HoldNote GetNote()
    {
        var holdNote = GetInstance();
        holdNote.State =  HoldNote.InputState.Idle;
        holdNote.Grade = NoteGrade.None;
        holdNote.SetWidth(2.5f);
        return holdNote;
    }
}
