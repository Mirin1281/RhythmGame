using UnityEngine;

namespace NoteCreating
{
    public class HoldNotePool : PoolBase<HoldNote>
    {
        public HoldNote GetNote(Lpb length)
        {
            var n = GetInstance();

            n.Refresh();
            n.SetLength(length);
            n.transform.SetParent(this.transform);

            return n;
        }
    }
}