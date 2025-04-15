using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    // ホールドが着地すると停止し、その後縦方向に判定が発生します
    [AddTypeMenu(FumenPathContainer.NoteCreate + "シャウトホールド", -60), System.Serializable]
    public class F_ShoutHold : NoteCreateBase<NoteData>
    {
        [SerializeField] Lpb startLpb = new Lpb(16);
        [SerializeField] int judgeCount = 6;
        [SerializeField] Lpb judgeInterval = new Lpb(64);
        [SerializeField] bool createSlide;
        [SerializeField] float deg;

        [SerializeField] TransformConverter transformConverter;

        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Hold, length: new Lpb(2)) };
        protected override NoteData[] NoteDatas => noteDatas;

        (Vector3, float) PosAndRotFunc(float t, NoteData data)
        {
            var dir = (deg + 90) * Mathf.Deg2Rad;
            var vec = new Vector3(Mathf.Cos(dir), Mathf.Sin(dir));
            var pos = new Vector3(data.X, 0) + (MoveTime - t) * Speed * vec;
            return (pos, deg);
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            (Vector3, float) posAndRotFunc(float t) => (new Vector3(data.X, (MoveTime - t) * Speed), 0);

            if (note is HoldNote hold)
            {
                var holdLength = note.Type == RegularNoteType.Hold ? startLpb : data.Length;
                note.SetPosAndRot(PosAndRotFunc(MoveTime, data));
                transformConverter.Convert(
                    note, mirror,
                    Time + MoveTime - Delta, MoveTime,
                    data.Option1, data.Option2);
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, note.GetPos(), MoveTime - Delta, holdLength, NoteJudgeStatus.ExpectType.Static));

                MoveAndJudge(hold, data).Forget();
            }
            else
            {
                // 着弾地点を設定 //
                note.SetPosAndRot(posAndRotFunc(MoveTime));
                transformConverter.Convert(
                    note, mirror,
                    Time + MoveTime - Delta, MoveTime,
                    data.Option1, data.Option2);
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, note.GetPos(), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));

                MoveNote(note, data);
            }
        }

        UniTask MoveNote(RegularNote note, NoteData data)
        {
            // ノーツの生存時間を求める //
            float lifeTime = MoveTime + 0.2f;
            if (data.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            return WhileYieldAsync(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                var (pos, rot) = PosAndRotFunc(t, data);
                note.SetPosAndRot(pos, rot);
                if (note is HoldNote hold)
                {
                    hold.SetLength(data.Length * Speed);
                    hold.SetMaskPos(new Vector2(pos.x, 0));
                }

                // 座標変換 //
                transformConverter.Convert(
                    note, mirror,
                    Time, t,
                    data.Option1, data.Option2);
            });
        }

        async UniTaskVoid MoveAndJudge(HoldNote hold, NoteData data)
        {
            hold.SetRot(deg);
            hold.SetMaskRot(0);

            MoveNote(hold, data).Forget();
            float delta = await Wait(MoveLpb, Delta);

            if (createSlide)
            {
                for (int i = 0; i < judgeCount; i++)
                {
                    var (pos, expectTime) = GetPosAndExpectTime(i, data, delta);
                    var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                    slide.SetPos(pos);
                    slide.SetRot(deg);
                    slide.IsVerticalRange = true;
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                        slide, pos, expectTime));
                }
            }
            else
            {
                for (int i = 0; i < judgeCount; i++)
                {
                    var (pos, expectTime) = GetPosAndExpectTime(i, data, delta);
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                        RegularNoteType.Slide, pos, expectTime, isVerticalRange: true, deg));
                }
            }
        }

        (Vector2, float) GetPosAndExpectTime(int i, NoteData data, float delta)
        {
            float y = i * data.Length.Time * Speed / judgeCount;
            Vector2 pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector2(data.X, y), deg, new Vector2(data.X, 0)));
            float expectTime = judgeInterval.Time * i + startLpb.Time - delta;
            return (pos, expectTime);
        }
    }
}