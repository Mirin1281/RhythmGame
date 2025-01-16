using UnityEngine;

namespace NoteCreating
{
    public class LinePool : PoolBase<Line>
    {
        public Line GetLine()
        {
            var line = GetInstance();

            line.SetWidth(20f);
            line.SetHeight(0.06f);
            line.transform.localRotation = default;
            line.transform.SetParent(this.transform);
            line.SetRendererEnabled(true);
            line.SetAlpha(0.25f);

            return line;
        }
    }
}