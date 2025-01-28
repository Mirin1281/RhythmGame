using UnityEngine;

namespace NoteCreating
{
    public class SlideNote : RegularNote
    {
        public override RegularNoteType Type => RegularNoteType.Slide;
        static readonly float baseAlpha = 0.4f;

        public override float GetAlpha()
        {
            return spriteRenderer.color.a / baseAlpha;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha * baseAlpha);
        }
    }
}