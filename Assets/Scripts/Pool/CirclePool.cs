using UnityEngine;

namespace NoteCreating
{
    public class CirclePool : PoolBase<Circle>
    {
        public Circle GetCircle()
        {
            var circle = GetInstance();

            circle.Refresh();
            circle.transform.SetParent(this.transform);

            return circle;
        }
    }
}