using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("AprilRabbit/5レーン"), System.Serializable]
    public class F_AprilRabbit_5Lane : Generator_Common
    {
        protected override async UniTask GenerateAsync()
        {
            UniTask.Void(async () => 
            {
                await Wait(4, 4, delta: 0);
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

        F_AprilRabbit_5Lane Note(int x, NoteType type = NoteType.Normal, float delta = -1, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.GetNote2D(type, parentTs);
            note.SetWidth(1.2f);
            int dir = 10 * x;
            note.SetRotate(dir);
            Vector3 toPos = x switch
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
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(note, toPos, expectTime, mode: NoteExpect.ExpectMode.Static);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(note, toPos + pos, expectTime, mode: NoteExpect.ExpectMode.Static);
            }
            return this;


            async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
            {
                if(delta == -1)
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
                    await Helper.Yield();
                }
            }
        }

        void Line(float dir, Vector3 pos)
        {
            var line = Helper.GetLine();
            line.SetRotate(dir);
            line.SetPos(pos);
            line.FadeIn(0.5f, 0.6f);
            UniTask.Void(async () => 
            {
                await Helper.WaitSeconds(11);
                line.FadeOut(0.5f);
            });
        }

        UniTask Wait(float lpb) => base.Wait(lpb);

        protected override string GetName()
        {
            return "5Lane";
        }

        protected override string GetSummary()
        {
            return GetInverseSummary();
        }
    }
}
