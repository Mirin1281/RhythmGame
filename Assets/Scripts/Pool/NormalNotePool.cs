using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    public NormalNote GetNote(int index = 0)
    {
        return InitNote(GetInstance(index));


        static NormalNote InitNote(NormalNote note)
        {
            return note;
        }
    }
}
