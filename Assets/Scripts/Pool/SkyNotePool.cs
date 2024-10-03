using System.Collections.Generic;
using UnityEngine;

public class SkyNotePool : PoolBase<SkyNote> 
{
    [SerializeField] GameObject lane3D;
    
    public SkyNote GetNote(int index = 0)
    {
        var n = GetInstance(index);
        n.SetRotate(0);
        n.SetWidth(1f);
        n.transform.localScale = new Vector3(3.8f, 0.4f, 1f);
        n.SetRendererEnabled(true);
        n.transform.SetParent(lane3D.transform);
        return n;
    }

    public List<SkyNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }
}
