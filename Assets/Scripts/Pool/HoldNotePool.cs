using UnityEngine;

namespace NoteCreating
{
    public class HoldNotePool : PoolBase<HoldNote>
    {
        public HoldNote GetNote(Lpb length)
        {
            var n = GetInstance();

            n.State = HoldNote.InputState.Idle;
            n.SetRot(0);
            n.transform.SetLocalPositionAndRotation(default, default);
            n.SetLength(length);
            n.SetWidth(1f);
            n.SetMaskLength(5f);
            n.SetMaskPos(Vector2.zero);
            n.transform.localScale = Vector3.one;
            n.SetRendererEnabled(true);
            n.SetAlpha(1f);
            n.transform.SetParent(this.transform);
            n.IsVerticalRange = false;

            return n;
        }
    }
}