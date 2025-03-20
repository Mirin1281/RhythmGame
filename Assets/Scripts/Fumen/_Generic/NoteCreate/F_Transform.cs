using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "座標移動", -60), System.Serializable]
    public class F_Transform : NoteCreateBase<NoteData>
    {
        [SerializeField] Lpb moveLpb = new Lpb(1);
        [SerializeField] EaseType easeType = EaseType.Default;
        [SerializeField] float deg;
        [SerializeField] Vector2 pos;

        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: -1, option2: -1) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            var expectPos = EasingTransformGroupNote(note, data, lifeTime, deg, pos, moveLpb.Time, easeType);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, expectPos, MoveTime - Delta, data.Length, ExpectType.Y_Static));
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            return;
        }

        Vector2 EasingTransformGroupNote(RegularNote note, NoteData data, float lifeTime, float toDeg, Vector2 toPos, float easeTime, EaseType easeType)
        {
            var dEasing = new Easing(0, toDeg, easeTime, easeType);
            var pEasing = new EasingVector2(Vector2.zero, toPos, easeTime, easeType);

            // 先頭を基準とした着地時間、着地座標を求める
            float a = Mathf.Clamp(Time, 0, easeTime);
            float d = dEasing.Ease(a);
            Vector2 p = pEasing.Ease(a);

            var expectPos = mirror.Conv(p + MyUtility.GetRotatedPos(new Vector2(data.X, 0), d));

            float w = Time - MoveTime;

            // 移動
            WhileYield(lifeTime, t =>
            {
                float d;
                Vector2 p;
                if (t < easeTime + MoveTime)
                {
                    float a = Mathf.Clamp(t + w, 0, easeTime);
                    d = dEasing.Ease(a);
                    p = pEasing.Ease(a);
                }
                else
                {
                    d = toDeg;
                    p = toPos;
                }
                note.SetRot(mirror.Conv(d));
                var basePos = MyUtility.GetRotatedPos(new Vector2(data.X, (MoveTime - t) * Speed), d);
                note.SetPos(mirror.Conv(basePos + p));
                if (note.Type == RegularNoteType.Hold)
                {
                    var hold = note as HoldNote;
                    hold.SetMaskPos(mirror.Conv(MyUtility.GetRotatedPos(new Vector2(data.X, 0), d) + p));
                }
            });
            return expectPos;
        }
#if UNITY_EDITOR
        protected override string GetSummary()
        {
            return base.GetSummary() + "   Dir: " + deg;
        }
#endif
    }
}