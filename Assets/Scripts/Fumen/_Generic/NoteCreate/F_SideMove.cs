using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "途中で横移動", -60), System.Serializable]
    public class F_SideMove : NoteCreateBase<NoteData>
    {
        [SerializeField] Lpb moveLpb = new Lpb(4);

        [Header("オプション1 : 移動前のx座標")]
        [Header("オプション2 : 生成されてから移動するまでの時間(LPB)")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option2: 1) };
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

            var easing = new Easing(data.Option1, data.X, moveLpb.Time, EaseType.OutQuad);
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                float x;
                if (t < new Lpb(data.Option2).Time)
                {
                    x = data.Option1;
                }
                else if (t < new Lpb(data.Option2).Time + moveLpb.Time)
                {
                    float t2 = t - new Lpb(data.Option2).Time;
                    x = easing.Ease(t2);
                }
                else
                {
                    x = data.X;
                }
                note.SetPos(mirror.Conv(new Vector3(x, (MoveTime - t) * Speed)));
            });
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "横移動";
        }
#endif
    }
}