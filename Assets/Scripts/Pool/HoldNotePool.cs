using System.Collections.Generic;
using UnityEngine;

public class HoldNotePool : PoolBase<HoldNote>
{
    public HoldNote GetNote()
    {
        var n = GetInstance();
        n.State =  HoldNote.InputState.Idle;
        n.SetRotate(Vector3.zero);
        n.SetRotate(0);
        n.SetWidth(1f);
        n.transform.localScale = Vector3.one;
        n.SetRendererEnabled(true);
        n.SetAlpha(1f);
        n.transform.SetParent(this.transform);
        return n;
    }
}
