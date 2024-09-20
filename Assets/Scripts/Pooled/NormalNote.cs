using UnityEngine;

public class NormalNote : NoteBase_2D
{
    static readonly float BaseScaleX = 3.6f;
    public override void SetWidth(float width)
    {
        Width = width;
        width *= BaseScaleX;
        SpriteRenderer.size = new Vector2(width, SpriteRenderer.size.y);
    }
}
