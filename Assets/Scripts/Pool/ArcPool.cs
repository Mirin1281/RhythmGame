using UnityEngine;

namespace NoteCreating
{
    public class ArcNotePool : PoolBase<ArcNote>
    {
        [SerializeField] GameObject lane3D;

        public ArcNote GetNote()
        {
            var n = GetInstance();

            n.transform.localRotation = default;
            n.SetRendererEnabled(true);
            n.transform.SetParent(lane3D.transform);
            n.SetRadius(0.5f);
            n.SetColor(true);

            return n;
        }
    }
}