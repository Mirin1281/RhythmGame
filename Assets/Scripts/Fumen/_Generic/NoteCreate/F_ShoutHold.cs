using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    // ホールドが着地すると停止し、その後縦方向に判定が発生します
    [AddTypeMenu(FumenPathContainer.NoteCreate + "シャウトホールド", -60), System.Serializable]
    public class F_ShoutHold : NoteCreateBase<NoteData>
    {
        [SerializeField] Lpb startLpb = new Lpb(16);
        [SerializeField] int judgeCount = 6;
        [SerializeField] Lpb judgeInterval = new Lpb(64);
        [SerializeField] bool createSlide;

        [Header("オプション1 : 角度")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Hold, length: new Lpb(2)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            if (data.Type != RegularNoteType.Hold)
            {
                DropAsync(note, mirror.Conv(data.X), lifeTime).Forget();
                return;
            }

            MoveAndJudge(note as HoldNote, data.X, data.Length, MoveTime + startLpb.Time, data.Option1).Forget();

            var holdLength = note.Type == RegularNoteType.Hold ? startLpb : data.Length;
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, new Vector2(data.X, 0), MoveTime - Delta, holdLength, NoteJudgeStatus.ExpectType.Y_Static));
        }

        async UniTaskVoid MoveAndJudge(HoldNote hold, float x, Lpb length, float lifeTime, float dir)
        {
            hold.SetRot(dir);
            hold.SetMaskRot(0);
            float dirX = Mathf.Cos((dir + 90) * Mathf.Deg2Rad);

            await WhileYieldAsync(MoveTime, t =>
            {
                if (hold.IsActive == false) return;
                hold.SetPos(mirror.Conv(new Vector3(x + dirX * (MoveTime - t) * Speed, (MoveTime - t) * Speed)));
            }, Delta);

            if (createSlide)
            {
                float dirY = Mathf.Sin((dir + 90) * Mathf.Deg2Rad);
                for (int i = 0; i < judgeCount; i++)
                {
                    float y = i * length.Time * Speed / judgeCount * dirY;
                    var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                    Vector2 pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector2(x, y), dir, new Vector2(x, 0)));
                    slide.SetPos(pos);
                    slide.SetRot(dir);
                    slide.IsVerticalRange = true;
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(slide, pos, judgeInterval.Time * i + startLpb.Time - Delta));
                }
            }
            else
            {
                for (int i = 0; i < judgeCount; i++)
                {
                    float y = i * length.Time * Speed / judgeCount;
                    Vector2 pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector2(x, y), dir, new Vector2(x, 0)));
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                        RegularNoteType.Slide, pos, judgeInterval.Time * i + startLpb.Time - Delta, isVerticalRange: true, dir));
                }
            }
        }
    }
}