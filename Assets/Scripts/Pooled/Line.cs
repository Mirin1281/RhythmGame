using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Line : PooledBase, ITransformable
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

    public void FadeIn(float time, float toAlpha = 1f, Easing easing = default)
    {
        SetAlpha(0);
        FadeAlphaAsync(toAlpha, time, easing).Forget();
    }

    public void FadeOut(float time, float delaySeconds = 0f, bool inActive = true, Easing easing = default, float delta = 0)
    {
        UniTask.Void(async () => 
        {
            await MyUtility.WaitSeconds(delaySeconds, destroyCancellationToken);
            await FadeAlphaAsync(0, time, easing);
            if(inActive)
            {
                SetActive(false);
            }
        });
    }

    public async UniTask MoveAsync(Vector3 toPos, float time, float delta = 0)
    {
        var xEasing = new Easing(GetPos().x, toPos.x, time, EaseType.OutQuad);
        var yEasing = new Easing(GetPos().y, toPos.y, time, EaseType.OutQuad);
        var zEasing = new Easing(GetPos().z, toPos.z, time, EaseType.OutQuad);

        float t = delta;
        while(t < time)
        {
            t += Time.deltaTime;
            SetPos(new Vector3(xEasing.Ease(t), yEasing.Ease(t), zEasing.Ease(t)));
            await UniTask.Yield(destroyCancellationToken);
        }
    }
    public async UniTask RotateAsync(float toRot, float time, float delta = 0)
    {
        var easing = new Easing(transform.eulerAngles.z, toRot, time, EaseType.OutQuad);

        float t = delta;
        while(t < time)
        {
            t += Time.deltaTime;
            SetRotate(easing.Ease(t));
            await UniTask.Yield(destroyCancellationToken);
        }
    }
}
