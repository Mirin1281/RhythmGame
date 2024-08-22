using UnityEngine;

public class FlickNotePool : PoolBase<FlickNote> 
{
    public FlickNote GetNote()
    {
        var flickNote = GetInstance();
        flickNote.SetRotate(0);
        flickNote.SetWidth(3f);
        return flickNote;
    }
}
