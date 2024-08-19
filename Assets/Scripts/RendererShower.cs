using UnityEngine;
using Cysharp.Threading.Tasks;

public class RendererShower : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] MeshRenderer[] meshRenderers;
    [SerializeField] bool startShow;

    void Start()
    {
        if(startShow)
        {
            ShowLaneAsync(0f);
        }
        else
        {
            HideLaneAsync(0f);
        }
    }

    public UniTask ShowLaneAsync(float time, EaseType easeType = EaseType.OutQuad)
    {
        return FadeLaneAsync(0, 1, time, easeType);
    }
    public UniTask HideLaneAsync(float time, EaseType easeType = EaseType.OutQuad)
    {
        return FadeLaneAsync(1, 0, time, easeType);
    }

    async UniTask FadeLaneAsync(float start, float from, float time, EaseType easeType)
    {
        var easing = new Easing(start, from, time, easeType);
        var t = 0f;
        while (t < time)
        {
            SetAlpha(easing.Ease(t), spriteRenderers, meshRenderers);
            t += Time.deltaTime;
            await UniTask.Yield();
        }
        SetAlpha(from, spriteRenderers, meshRenderers);
    }

    static void SetAlpha(float alpha, SpriteRenderer[] spriteRenderers, MeshRenderer[] meshRenderers)
    {
        foreach(var spriteRenderer in spriteRenderers)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }
        foreach(var meshRenderer in meshRenderers)
        {
            var c = meshRenderer.sharedMaterial.color;
            meshRenderer.sharedMaterial.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}
