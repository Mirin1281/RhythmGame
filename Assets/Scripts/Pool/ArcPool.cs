using UnityEngine;

namespace NoteCreating
{
    public class ArcNotePool : PoolBase<ArcNote>
    {
        public ArcNote GetNote()
        {
            var n = GetInstance();

            n.transform.localRotation = default;
            n.SetRendererEnabled(true);
            n.SetRadius(0.5f);
            n.SetAlpha(0.8f);

            return n;
        }
    }
}