using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcNotePool : PoolBase<ArcNote>
{
    [SerializeField] GameObject lane3D;

    public ArcNote GetNote()
    {
        var arcNote = GetInstance();
        arcNote.transform.parent = lane3D.transform;
        arcNote.transform.localRotation = default;
        return arcNote;
    }

    public List<ArcNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
