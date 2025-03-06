using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "タップホールド", -60), System.Serializable]
    public class F_TouchHold : NoteCreateBase<NoteData>
    {
        [Header("ホールドがタッチ判定になり、すぐに消えます")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, pos, MoveTime - Delta, default, expectType));
        }
    }
}