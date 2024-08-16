using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    [SerializeField] GameObject lane3D;
    
    public NormalNote GetNote(int index = 0)
    {
        if(index == 1)
        {
            var skyNote = GetInstance(index);
            skyNote.transform.parent = lane3D.transform;
            skyNote.SetWidth(3.5f);
            return skyNote;
        }

        var normalNote = GetInstance(index);
        normalNote.SetWidth(3f);
        return normalNote;
    }
}
