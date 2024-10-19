using System.Collections.Generic;
using UnityEngine;

public class CircleNotePool : PoolBase<CircleNote> 
{
    [SerializeField] Sprite circleSprite;
    //public bool isDarkMode;

    public CircleNote GetNote(int index = 0)
    {
        var n = GetInstance(index);
        n.transform.localScale = Vector3.one;
        n.SetRendererEnabled(true);
        n.SetSprite(circleSprite);
        n.SetAlpha(0.7f);
        //n.SetColor(isDarkMode ? Color.white : Color.black);
        n.transform.SetParent(this.transform);
        return n;
    }
}
