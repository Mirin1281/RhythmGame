using System.Collections.Generic;
using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote> 
{
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite simultaneousSprite;

    public NormalNote GetNote(int index = 0, bool isSimultaneous = false)
    {
        var normalNote = GetInstance(index);
        normalNote.SetRotate(0);
        normalNote.SetSprite(defaultSprite);
        normalNote.SetWidth(1f);
        normalNote.SetRendererEnabled(true);
        normalNote.transform.SetParent(this.transform);
        if(index != 1)
        {
            //normalNote.SetWidth(3f);
        }
        return normalNote;
    }

    public List<NormalNote> GetAllNotes(int index = 0)
    {
        return PooledTable[index];
    }

    public Sprite GetSimultaneousSprite() => simultaneousSprite;
}
