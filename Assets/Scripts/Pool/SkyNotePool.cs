using System.Collections.Generic;
using UnityEngine;

public class SkyNotePool : PoolBase<SkyNote> 
{
    [SerializeField] GameObject lane3D;
    
    public SkyNote GetNote(int index = 0)
    {
        var skyNote = GetInstance(index);
        skyNote.transform.parent = lane3D.transform;
        //skyNote.SetWidth(3.5f);
        return skyNote;
    }

    public List<SkyNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
