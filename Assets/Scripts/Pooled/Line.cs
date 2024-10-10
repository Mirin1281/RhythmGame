using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Line : PooledBase
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public virtual Vector3 GetPos(bool isWorld = false)
    {
        if(isWorld)
        {
            return transform.position;
        }
        else
        {
            return transform.localPosition;
        }
    }
    public virtual void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public virtual void SetRotate(float deg)
    {
        transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }

    public void SetWidth(float width)
    {
        transform.localScale = new Vector3(width, transform.localScale.y);
    }
    public void SetHeight(float height)
    {
        transform.localScale = new Vector3(transform.localScale.x, height);
    }

    public void SetRendererEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
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
}
