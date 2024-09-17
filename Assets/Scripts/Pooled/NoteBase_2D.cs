using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class NoteBase_2D : NoteBase
{
    [SerializeField] SpriteRenderer spriteRenderer;
    protected SpriteRenderer SpriteRenderer => spriteRenderer;

    public override void SetRendererEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }

    public virtual void SetWidth(float width)
    {
        var scale = transform.localScale;
        transform.localScale = new Vector3(width, scale.y, scale.z);
    }

    public void SetAlpha(float alpha)
    {
        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
    }

    public async UniTask FadeAlphaAsync(float toAlpha, float time, Easing easing = default)
    {
        if(easing.EaseType == EaseType.None)
        {
            easing = new Easing(spriteRenderer.color.a, toAlpha, time, EaseType.OutQuad);
        }
        var t = 0f;
        while (t < time)
        {
            SetAlpha(easing.Ease(t));
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        SetAlpha(toAlpha);
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
