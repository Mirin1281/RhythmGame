using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public class Circle : ItemBase
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Transform maskTs;
        float width = 0.15f;
        static readonly float scaleOffset = 0.404f;

        public override void SetRendererEnabled(bool enabled)
        {
            spriteRenderer.enabled = enabled;
        }

        public override float GetAlpha()
        {
            return spriteRenderer.color.a;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        public float GetScale()
        {
            return (spriteRenderer.transform.localScale.x - width / 2f) / scaleOffset;
        }

        public void SetScale(float scale)
        {
            float half = width / 2f;
            spriteRenderer.transform.localScale = (scale + half) * scaleOffset * Vector3.one;
            maskTs.localScale = (scale - half) * scaleOffset * Vector3.one;
        }

        public void SetWidth(float width)
        {
            float s = GetScale();
            this.width = width;
            float half = width / 2f;
            spriteRenderer.transform.localScale = (s + half) * scaleOffset * Vector3.one;
            maskTs.localScale = (s - half) * scaleOffset * Vector3.one;
        }

        public void SetMaskEnabled(bool enabled)
        {
            maskTs.gameObject.SetActive(enabled);
        }

        public async UniTask SetScaleAsync(float endScale, float time, EaseType easeType = EaseType.InQuad, float delta = 0)
        {
            var easing = new Easing(GetScale(), endScale, time, easeType);
            float baseTime = Metronome.Instance.CurrentTime - delta;
            while (true)
            {
                float t = Metronome.Instance.CurrentTime - baseTime;
                SetScale(easing.Ease(t));
                if (t >= time) break;
                await UniTask.Yield(destroyCancellationToken);
            }
            SetScale(endScale);
        }

        public void Refresh()
        {
            transform.localScale = Vector3.one;
            SetRendererEnabled(true);
            SetAlpha(0.7f);
        }
    }
}