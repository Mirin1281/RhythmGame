using NoteCreating;
using UnityEngine;

public class DebugSphere : ItemBase
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public override float GetAlpha()
    {
        throw new System.NotImplementedException();
    }

    public override void SetAlpha(float alpha)
    {
        throw new System.NotImplementedException();
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public override void SetRendererEnabled(bool enabled)
    {
        throw new System.NotImplementedException();
    }
}
