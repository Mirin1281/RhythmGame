using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "AprilRabbit/5レーン"), System.Serializable]
    public class F_AprilRabbit_5Lane : CommandBase
    {
        [SerializeField] Mirror mirror;

        protected override async UniTaskVoid ExecuteAsync()
        {
            UniTask.Void(async () =>
            {
                await Wait(1, delta: 0);
                Line(20, new Vector3(8, 1.5f));
                await Wait(8, delta: 0);
                Line(10, new Vector3(4, 0.5f));
                await Wait(8, delta: 0);
                Line(0, new Vector3(0, 0));
                await Wait(8, delta: 0);
                Line(-10, new Vector3(-4, 0.5f));
                await Wait(8, delta: 0);
                Line(-20, new Vector3(-8, 1.5f));
            });

            await Note(2).Wait(8);
            await Note(1).Wait(8);
            await Note(0).Wait(8);
            await Note(-1).Wait(8);
            await Note(2).Note(0).Wait(8);
            await Note(-2).Wait(16);
            await Note(1).Wait(16);
            await Note(0).Wait(8);
            await Note(-1).Wait(8);
            await Note(2).Note(0).Wait(8);
            await Note(1).Wait(8);
            await Note(2).Note(-2).Wait(8);
            await Note(0).Wait(8);
            await Note(-2).Note(0).Wait(8);
            await Note(2).Wait(8);
            await Note(2).Note(-1).Wait(8);
            await Note(0).Wait(8);

            await Note(-2).Wait(8);
            await Note(1).Wait(8);
            await Note(2).Wait(8);
            await Note(-1).Wait(8);
            await Note(2).Wait(8);
            await Note(-1).Wait(8);
            await Note(-2).Wait(8);
            await Note(1).Wait(8);
            await Note(-1).Wait(8);
            await Note(0).Wait(8);
            await Note(1).Wait(8);
            await Note(0).Wait(8);
            await Note(1).Note(2).Wait(8);
            await Note(0).Note(1).Wait(8);
            await Note(-1).Note(0).Wait(8);
            await Note(-2).Note(-1).Wait(8);

            await Note(2).Wait(8);
            await Note(1).Wait(8);
            await Note(0).Wait(8);
            await Note(-1).Wait(8);
            await Note(2).Note(0).Wait(8);
            await Note(2).Wait(16);
            await Note(-2).Wait(16);
            await Note(0).Wait(8);
            await Note(-1).Wait(8);
            await Note(2).Note(0).Wait(8);
            await Note(1).Wait(8);
            await Note(2).Note(-2).Wait(8);
            await Note(0).Wait(8);
            await Note(-2).Note(0).Wait(8);
            await Note(2).Wait(8);
            await Note(2).Note(-1).Wait(8);
            await Note(0).Wait(8);

            await Note(1).Wait(8);
            await Note(-1).Wait(8);
            await Note(0).Wait(8);
            await Note(1).Wait(8);
            await Note(0).Wait(8);
            await Note(-1).Wait(8);
            await Note(1).Wait(8);
            await Note(0).Wait(8);
            await Note(1).Wait(8);
        }

        UniTask Wait(float lpb, float delta = -1)
        {
            return base.Wait(new Lpb(lpb), delta);
        }

        F_AprilRabbit_5Lane Note(int x, RegularNoteType type = RegularNoteType.Normal, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            var note = Helper.GetRegularNote(type);
            note.SetWidth(1.2f);
            int dir = 10 * mirror.Conv(x);
            note.SetRot(dir);
            Vector3 toPos = mirror.Conv(x) switch
            {
                -2 => new Vector3(-8, 1.5f),
                -1 => new Vector3(-4, 0.5f),
                0 => new Vector3(0, 0f),
                1 => new Vector3(4, 0.5f),
                2 => new Vector3(8, 1.5f),
                _ => throw new System.Exception()
            };
            Vector3 startPos = toPos + StartBase * new Vector3(Mathf.Cos((dir + 90) * Mathf.Deg2Rad), Mathf.Sin((dir + 90) * Mathf.Deg2Rad));
            DropAsync(note, startPos, delta).Forget();

            float expectTime = StartBase / Speed - delta;
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, toPos, expectTime, expectType: NoteJudgeStatus.ExpectType.Static));
            return this;


            async UniTask DropAsync(RegularNote note, Vector3 startPos, float delta = -1)
            {
                if (delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                var vec = Speed * new Vector3(Mathf.Cos((dir + 270) * Mathf.Deg2Rad), Mathf.Sin((dir + 270) * Mathf.Deg2Rad));
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + time * vec);
                    await Yield();
                }
            }
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
                await WaitSeconds(11);
                line.FadeOut(0.5f);
            });
        }

        //UniTask Wait(float lpb) => base.Wait(lpb);

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "5Lane";
        }

        protected override string GetSummary()
        {
            return mirror.GetStatusText();
        }
#endif
    }
}
