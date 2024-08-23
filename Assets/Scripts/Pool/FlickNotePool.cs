using System.Collections.Generic;
using UnityEngine;

public class FlickNotePool : PoolBase<FlickNote> 
{
    public FlickNote GetNote()
    {
        var flickNote = GetInstance();
        flickNote.SetRotate(0);
        flickNote.SetWidth(3f);
        return flickNote;
    }

    public List<FlickNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
