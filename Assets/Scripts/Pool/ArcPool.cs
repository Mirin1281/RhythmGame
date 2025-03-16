using UnityEngine;

namespace NoteCreating
{
    public class ArcNotePool : PoolBase<ArcNote>
    {
        public ArcNote GetNote()
        {
            var n = GetInstance();

            n.Refresh();

            return n;
        }
    }
}