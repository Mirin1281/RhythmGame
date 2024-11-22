using System.Collections.Generic;
using UnityEngine;

public class NormalNotePool : PoolBase<NormalNote>
{
    [Space(20)]
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite simultaneousSprite;
    //public bool isDarkMode;

    public NormalNote GetNote(int index = 0, bool isSimultaneous = false)
    {
        var n = GetInstance(index);
        n.SetRotate(0);
        n.SetWidth(1f);
        n.transform.localScale = Vector3.one;
        n.SetRendererEnabled(true);
        n.SetSprite(defaultSprite);
        n.SetAlpha(1f);
        //n.SetColor(isDarkMode ? Color.white : Color.black);
        n.transform.SetParent(this.transform);
        n.IsVerticalRange = false;
        return n;
    }

    public Sprite GetSimultaneousSprite() => simultaneousSprite;
}
