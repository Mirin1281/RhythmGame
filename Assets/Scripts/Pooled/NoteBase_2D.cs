using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteBase_2D : NoteBase
{
    [SerializeField] SpriteRenderer spriteRenderer;
    protected SpriteRenderer SpriteRenderer => spriteRenderer;

    public override void SetRendererEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
