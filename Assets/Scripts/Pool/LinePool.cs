using UnityEngine;

namespace NoteCreating
{
    public class LinePool : PoolBase<Line>
    {
        public Line GetLine()
        {
            var line = GetInstance();

            line.SetWidth(50f);
            line.SetHeight(0.1f);
            line.transform.localRotation = default;
            line.transform.SetParent(this.transform);
            line.SetRendererEnabled(true);
            line.SetAlpha(1);

            return line;
        }
    }
}