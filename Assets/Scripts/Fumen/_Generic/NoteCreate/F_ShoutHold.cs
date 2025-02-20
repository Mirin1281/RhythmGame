using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    // ホールドが着地すると停止し、その後縦方向に判定が発生します
    [AddTypeMenu(FumenPathContainer.NoteCreate + "シャウトホールド", -60), System.Serializable]
    public class F_ShoutHold : NoteCreateBase<NoteData>
    {
        [SerializeField] int judgeCount = 6;
        [SerializeField] Lpb judgeInterval = new Lpb(8);
        [SerializeField] bool createSlide;

        [Header("オプション1 : 角度")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Hold, length: new Lpb(2)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }

            if (data.Type != RegularNoteType.Hold)
            {
                AddExpect();
                DropAsync(note, mirror.Conv(data.X)).Forget();
                return;
            }

            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, default, MoveTime - Delta, judgeInterval, ExpectType.Y_Static));
            MoveAndJudge(note as HoldNote, data.X, data.Length, MoveTime + 0.1f, data.Option1).Forget();
        }

        async UniTaskVoid MoveAndJudge(HoldNote hold, float x, Lpb length, float lifeTime, float dir)
        {
            hold.SetRot(dir);
            hold.SetMaskRot(0);
            float dirX = Mathf.Cos((dir + 90) * Mathf.Deg2Rad);
            float dirY = Mathf.Sin((dir + 90) * Mathf.Deg2Rad);

            if (createSlide)
            {
                WhileYield(lifeTime, t =>
                {
                    if (hold.IsActive == false) return;
                    hold.SetPos(mirror.Conv(new Vector3(x + dirX * (MoveTime - t) * Speed, (MoveTime - t) * Speed)));
                }, Delta);
                var delta = await WaitSeconds(lifeTime, Delta);

                Lpb interval = judgeInterval / 8f;
                for (int i = 0; i < judgeCount; i++)
                {
                    float y = i * length.Time * Speed / judgeCount * dirY;
                    var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                    Vector2 pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector2(x, y), dir, new Vector2(x, 0)));
                    slide.SetPos(pos);
                    slide.SetRot(dir);
                    slide.IsVerticalRange = true;
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(slide, pos, interval.Time * i - delta));
                }
            }
            else
            {
                float delta = await WhileYieldAsync(lifeTime, t =>
                {
                    if (hold.IsActive == false) return;
                    hold.SetPos(mirror.Conv(new Vector3(x + dirX * (MoveTime - t) * Speed, (MoveTime - t) * Speed)));
                }, Delta);

                Lpb interval = judgeInterval / 8f;
                for (int i = 0; i < judgeCount; i++)
                {
                    float y = i * length.Time * Speed / judgeCount;
                    Vector2 pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector2(x, y), dir, new Vector2(x, 0)));
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                        RegularNoteType.Slide, pos, interval.Time * i - delta, isVerticalRange: true, dir));
                }
            }
        }
    }
}