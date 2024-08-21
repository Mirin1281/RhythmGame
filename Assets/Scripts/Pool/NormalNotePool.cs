using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    public NormalNote GetNote(int index = 0)
    {
        var normalNote = GetInstance(index);
        normalNote.SetRotate(0);
        if(index != 1)
        {
            normalNote.SetWidth(3f);
        }
        return normalNote;
    }
}
