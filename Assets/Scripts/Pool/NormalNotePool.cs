using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    public NormalNote GetNote(int index = 0)
    {
        var normalNote = GetInstance(index);
        if(index != 1)
        {
            normalNote.SetWidth(3f);
        }
        return normalNote;
    }
}
