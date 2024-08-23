using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNote : NoteBase
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void SetWidth(float width)
    {
        spriteRenderer.size = new Vector2(width, spriteRenderer.size.y);
    }

    public override void SetRendererEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }
}
