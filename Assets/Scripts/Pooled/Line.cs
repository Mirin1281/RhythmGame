using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public class Line : ItemBase
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        public static float BaseAlpha = 1f;

        public void SetWidth(float width)
        {
            transform.localScale = new Vector3(width, transform.localScale.y);
        }
        public void SetHeight(float height)
        {
            transform.localScale = new Vector3(transform.localScale.x, height);
        }

        public override void SetRendererEnabled(bool enabled)
        {
            spriteRenderer.enabled = enabled;
        }

        public override float GetAlpha()
        {
            return spriteRenderer.color.a / BaseAlpha;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha * BaseAlpha);
        }

        public void Refresh()
        {
            SetWidth(50f);
            SetHeight(0.1f);
            transform.localRotation = default;
            SetRendererEnabled(true);
            SetAlpha(1);
        }
    }
}