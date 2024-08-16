using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class FlickNote : NoteBase
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void SetWidth(float width)
    {
        spriteRenderer.size = new Vector2(width, spriteRenderer.size.y);
    }
}
