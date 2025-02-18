using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "ノーツ回転", -60), System.Serializable]
    public class F_RotateNote : NoteCreateBase<NoteData>
    {
        [SerializeField] bool isGroup;
        [SerializeField] Lpb[] rotateTimings;
        [SerializeField] Lpb timing = new Lpb(4) * 5;
        [SerializeField] Lpb rotateLpb = new Lpb(8);

        [Header("オプション1 : 回転係数")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 1) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }

            AddExpect();
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            if (isGroup)
            {
                WhileYieldGroupAsync(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));
                },
                rotateTimings, status =>
                {
                    if (note.IsActive == false || note.Type == RegularNoteType.Hold) return;
                    var easing = new Easing(0, (status.index % 2 == 0 ? 180 : -180) * data.Option1, rotateLpb.Time, EaseType.OutQuad);
                    easing.EaseAsync(Helper.Token, status.d, t => note.SetRot(t)).Forget();
                }).Forget();
            }
            else
            {
                var easing = new Easing(0, 180 * data.Option1, rotateLpb.Time, EaseType.OutQuad);
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    if (t > timing.Time && note.Type != RegularNoteType.Hold)
                    {
                        float t2 = t - timing.Time;
                        note.SetRot(easing.Ease(Mathf.Clamp(t2, 0, rotateLpb.Time)));
                    }
                    note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                });
            }
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "RotateNote";
        }
#endif
    }
}