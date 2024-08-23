using UnityEngine;

public class NormalNote : NoteBase
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
