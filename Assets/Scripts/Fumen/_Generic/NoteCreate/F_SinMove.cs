using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "左右に円運動", -50), System.Serializable]
    public class F_SinMove : NoteCreateBase<NoteData>
    {
        [SerializeField] float amp = 1;
        [SerializeField] Lpb frequency = new Lpb(2);
        [SerializeField] float phaseDeg = 0;
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            Lpb w = WaitDelta;
            if (note.Type == RegularNoteType.Hold)
            {
                var hold = note as HoldNote;
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    float x = mirror.Conv(data.X + amp * Mathf.Sin((t + w.Time) * (2f / frequency.Time) * Mathf.PI + phaseDeg * Mathf.Deg2Rad));
                    note.SetPos(new Vector3(x, (MoveTime - t) * Speed));
                    hold.SetMaskPos(new Vector2(x, 0));
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    float x = data.X + amp * Mathf.Sin((t + w.Time) * (2f / frequency.Time) * Mathf.PI + phaseDeg * Mathf.Deg2Rad);
                    note.SetPos(mirror.Conv(new Vector3(x, (MoveTime - t) * Speed)));
                });
            }
        }
    }
}