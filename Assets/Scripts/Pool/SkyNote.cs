using UnityEngine;

public class SkyNote : NoteBase
{
    [SerializeField] MeshRenderer meshRenderer;

    public void SetWidth(float width)
    {
        var scale = transform.localScale;
        transform.localScale = new Vector3(width, scale.y, scale.z);
    }

    public void SetRendererEnabled(bool enabled)
    {
        meshRenderer.enabled = enabled;
    }
}
