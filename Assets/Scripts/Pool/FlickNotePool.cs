using System.Collections.Generic;
using UnityEngine;

public class FlickNotePool : PoolBase<FlickNote> 
{
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite simultaneousSprite;

    public FlickNote GetNote()
    {
        var flickNote = GetInstance();
        flickNote.SetRotate(0);
        flickNote.SetSprite(defaultSprite);
        //flickNote.SetWidth(3f);
        return flickNote;
    }

    public List<FlickNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }

    public Sprite GetSimultaneousSprite() => simultaneousSprite;
}
