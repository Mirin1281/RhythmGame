using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostEffector : MonoBehaviour
{
    [SerializeField] Material _material;

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if(_material == null) return;
        Graphics.Blit(source, dest, _material);
    }
}