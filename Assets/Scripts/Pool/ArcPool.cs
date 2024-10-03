using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcNotePool : PoolBase<ArcNote>
{
    [SerializeField] GameObject lane3D;

    public ArcNote GetNote()
    {
        var n = GetInstance();
        n.transform.localRotation = default;
        n.SetRendererEnabled(true);
        n.transform.SetParent(lane3D.transform);
        return n;
    }

    public List<ArcNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
