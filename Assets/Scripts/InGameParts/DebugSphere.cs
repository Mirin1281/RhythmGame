using NoteCreating;
using UnityEngine;

public class DebugSphere : ItemBase
{
    [SerializeField] MeshRenderer meshRenderer;

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
        var material = meshRenderer.sharedMaterial;
        var newMat = new Material(material);
        newMat.color = color;
        meshRenderer.material = newMat;
    }

    public override void SetRendererEnabled(bool enabled)
    {
        throw new System.NotImplementedException();
    }
}
