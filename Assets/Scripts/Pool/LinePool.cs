using UnityEngine;

namespace NoteCreating
{
    public class LinePool : PoolBase<Line>
    {
        public Line GetLine()
        {
            var line = GetInstance();

            line.Refresh();
            line.transform.SetParent(this.transform);

            return line;
        }
    }
}