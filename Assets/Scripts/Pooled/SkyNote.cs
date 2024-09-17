using UnityEngine;

public class SkyNote : NoteBase
{
    [SerializeField] MeshRenderer meshRenderer;
    static readonly float BaseScaleX = 3.8f;

    public void SetWidth(float width)
    {
        Width = width;
        width *= BaseScaleX;
        var scale = transform.localScale;
        transform.localScale = new Vector3(width, scale.y, scale.z);
    }

    public override void SetRendererEnabled(bool enabled)
    {
        meshRenderer.enabled = enabled;
    }
}
