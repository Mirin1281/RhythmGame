using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNotePool : PoolBase<HoldNote>
{
    public HoldNote GetNote()
    {
        return InitNote(GetInstance());


        static HoldNote InitNote(HoldNote note)
        {
            note.State =  HoldNote.InputState.Idle;
            note.Grade = NoteGrade.None;
            return note;
        }
    }
}
