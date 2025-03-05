using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "AprilRabbit/5レーン(新)"), System.Serializable]
    public class F_AprilRabbit_New5Lane : NoteCreateBase<NoteData>
    {
        [SerializeField] bool isCreateLines = true;
        [SerializeField] Lpb lineLifeLpb = new Lpb(0.25f);

        [Header("Xは-2から2までの整数を入力してください")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override async UniTaskVoid ExecuteAsync()
        {
            UniTask Wait(float lpb)
            {
                return base.Wait(new Lpb(lpb), delta: 0);
            }
            void Line(float dir, Vector3 pos)
            {
                var line = Helper.GetLine();
                line.SetRot(mirror.Conv(dir));
                line.SetPos(mirror.Conv(pos));
                line.SetAlpha(0);
                line.FadeIn(0.5f, 0.6f);
                UniTask.Void(async () =>
                {
                    await base.Wait(lineLifeLpb);
                    line.FadeOut(0.5f);
                });
            }


            base.ExecuteAsync().Forget();

            if (isCreateLines == false) return;
            await Wait(1);
            Line(20, new Vector3(8, 1.5f));
            await Wait(8);
            Line(10, new Vector3(4, 0.5f));
            await Wait(8);
            Line(0, new Vector3(0, 0));
            await Wait(8);
            Line(-10, new Vector3(-4, 0.5f));
            await Wait(8);
            Line(-20, new Vector3(-8, 1.5f));
        }

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            int x = mirror.Conv(Mathf.RoundToInt(data.X));

            int dir = 10 * x;
            note.SetRot(dir);
            Vector3 toPos = x switch
            {
                -2 => new Vector3(-8, 1.5f),
                -1 => new Vector3(-4, 0.5f),
                0 => new Vector3(0, 0f),
                1 => new Vector3(4, 0.5f),
                2 => new Vector3(8, 1.5f),
                _ => throw new System.ArgumentOutOfRangeException($"value: {x}")
            };

            if (HoldNote.TryParse(note, out var hold))
            {
                hold.SetMaskPos(toPos);
                hold.SetMaskRot(dir);
            }

            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, toPos, MoveTime - Delta, data.Length, expectType: ExpectType.Static));

            Vector3 startPos = toPos + MoveTime * Speed * new Vector3(Mathf.Cos((dir + 90) * Mathf.Deg2Rad), Mathf.Sin((dir + 90) * Mathf.Deg2Rad));
            var vec = Speed * new Vector3(Mathf.Cos((dir + 270) * Mathf.Deg2Rad), Mathf.Sin((dir + 270) * Mathf.Deg2Rad));
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(startPos + t * vec);
            });
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            return;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "New5Lane";
        }
#endif
    }
}