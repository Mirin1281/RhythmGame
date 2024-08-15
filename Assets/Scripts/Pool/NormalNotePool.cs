using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    [SerializeField] GameObject lane3D;
    
    public NormalNote GetNote(int index = 0)
    {
        if(index == 1)
        {
            var note = GetInstance(index);
            note.transform.parent = lane3D.transform;
            return note;
        }
        return GetInstance(index);
    }
}
