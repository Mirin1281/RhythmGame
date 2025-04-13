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

        [SerializeField] TransformConverter transformConverter;

        [Header("オプション1 : 角度")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Hold, length: new Lpb(2)) };
        protected override NoteData[] NoteDatas => noteDatas;

        (Vector3, float) PosAndRotFunc(float t, NoteData data)
        {
            float dirX = Mathf.Cos((data.Option1 + 90) * Mathf.Deg2Rad);
            var pos = new Vector3(data.X + dirX * (MoveTime - t) * Speed, (MoveTime - t) * Speed);
            var rot = dirX;
            return (pos, rot);
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            (Vector3, float) posAndRotFunc(float t) => (new Vector3(data.X, (MoveTime - t) * Speed), 0);

            if (note is HoldNote hold)
            {
                MoveAndJudge(hold, data).Forget();

                var holdLength = note.Type == RegularNoteType.Hold ? startLpb : data.Length;
                note.SetPosAndRot(PosAndRotFunc(MoveTime, data));
                transformConverter.Convert(
                    note, mirror,
                    Time + MoveTime - Delta, MoveTime,
                    data.Option1, data.Option2);
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, note.GetPos(), MoveTime - Delta, holdLength, NoteJudgeStatus.ExpectType.Static));
            }
            else
            {
                MoveNote(note, data);

                // 着弾地点を設定 //
                note.SetPosAndRot(posAndRotFunc(MoveTime));
                transformConverter.Convert(
                    note, mirror,
                    Time + MoveTime - Delta, MoveTime,
                    data.Option1, data.Option2);
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, note.GetPos(), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));
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
            float x = data.X;
            Lpb length = data.Length;
            float dir = data.Option1;

            hold.SetRot(dir);
            hold.SetMaskRot(0);

            MoveNote(hold, data).Forget();
            await Wait(MoveLpb);

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