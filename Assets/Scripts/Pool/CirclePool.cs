using UnityEngine;

namespace NoteCreating
{
    public class CirclePool : PoolBase<Circle>
    {
        public Circle GetCircle()
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