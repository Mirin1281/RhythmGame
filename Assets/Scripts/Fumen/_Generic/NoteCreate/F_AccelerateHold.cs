using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "加速するホールド", -100), System.Serializable]
    public class F_AccelerateHold : NoteCreateBase<NoteData>
    {
        [SerializeField] EaseType easeType = EaseType.InQuad;

        [Header("オプション1 : ホールド終端時の速度(デフォルト1)")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Hold, length: new Lpb(2)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            if (data.Type != RegularNoteType.Hold)
            {
                DropAsync(note, mirror.Conv(data.X), lifeTime).Forget();
                return;
            }

            var hold = note as HoldNote;
            hold.SetLength(data.Length * Speed * data.Option1);

            var speedEasing = new Easing(Speed, data.Option1 * Speed, data.Length.Time, easeType);

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                if (t < MoveTime)
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                }
                else
                {
                    float t2 = t - MoveTime;
                    note.SetPos(mirror.Conv(new Vector3(data.X, -t2 * speedEasing.Ease(t2))));
                }
            });
        }

#if UNITY_EDITOR

        protected override string GetSummary()
        {
            return NoteDatas?.Length + mirror.GetStatusText();
        }
#endif
    }
}