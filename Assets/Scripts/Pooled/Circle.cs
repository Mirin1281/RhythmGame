using UnityEngine;

namespace NoteCreating
{
    public class Circle : ItemBase
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Transform maskTs;
        float width = 0.2f;

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

        public void SetScale(float scale)
        {
            float half = width / 2f;
            spriteRenderer.transform.localScale = Vector3.one * (scale + half);
            maskTs.localScale = Vector3.one * (scale - half);
        }

        public void SetWidth(float width)
        {
            this.width = width;
            float half = width / 2f;
            float s = spriteRenderer.transform.localScale.x;
            spriteRenderer.transform.localScale = Vector3.one * (s + half);
            maskTs.localScale = Vector3.one * (s - half);
        }
    }
}