using UnityEngine;

namespace NoteCreating
{
    public class SlideNote : RegularNote
    {
        public override RegularNoteType Type => RegularNoteType.Slide;
        public static float BaseAlpha = 0.3f;

        public override float GetAlpha()
        {
            return spriteRenderer.color.a / BaseAlpha;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha * BaseAlpha);
        }
    }
}