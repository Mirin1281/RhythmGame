using UnityEngine;

namespace NoteCreating
{
    public class CircleNotePool : PoolBase<Circle>
    {
        public Circle GetNote()
        {
            var n = GetInstance();

            n.transform.localScale = Vector3.one;
            n.SetRendererEnabled(true);
            n.SetAlpha(0.7f);
            n.transform.SetParent(this.transform);

            return n;
        }
    }
}