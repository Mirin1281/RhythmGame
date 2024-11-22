using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNotePool : PoolBase<SlideNote>
{
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite simultaneousSprite;

    public SlideNote GetNote()
    {
        var n = GetInstance();
        n.SetRotate(0);
        n.SetWidth(1f);
        n.transform.localScale = Vector3.one;
        n.SetRendererEnabled(true);
        n.SetSprite(defaultSprite);
        n.SetAlpha(0.5f);
        n.transform.SetParent(this.transform);
        n.IsVerticalRange = false;
        return n;
    }

    public Sprite GetSimultaneousSprite() => simultaneousSprite;
}
