using UnityEngine;

public class FlickNotePool : PoolBase<FlickNote> 
{
    public FlickNote GetNote()
    {
        return InitNote(GetInstance());


        static FlickNote InitNote(FlickNote note)
        {
            return note;
        }
    }
}
