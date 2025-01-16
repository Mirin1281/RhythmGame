using UnityEngine;

namespace NoteCreating
{
    public class HoldNotePool : PoolBase<HoldNote>
    {
        public HoldNote GetNote()
        {
            var n = GetInstance();

            n.State = HoldNote.InputState.Idle;
            n.SetRot(0);
            n.transform.SetLocalPositionAndRotation(default, default);
            n.SetWidth(1f);
            n.SetMaskLength(5f);
            n.SetMaskLocalPos(Vector2.zero);
            n.transform.localScale = Vector3.one;
            n.SetRendererEnabled(true);
            n.SetAlpha(1f);
            n.transform.SetParent(this.transform);
            n.IsVerticalRange = false;

            return n;
        }
    }
}