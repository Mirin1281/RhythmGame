using UnityEngine;

public class NormalNote : NoteBase
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void SetWidth(float width)
    {
        if(spriteRenderer == null) return;
        spriteRenderer.size = new Vector2(width, spriteRenderer.size.y);
    }
}
