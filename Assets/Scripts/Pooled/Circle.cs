using UnityEngine;

namespace NoteCreating
{
    public class Circle : ItemBase
    {
        [SerializeField] SpriteRenderer spriteRenderer;

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

        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }
    }
}