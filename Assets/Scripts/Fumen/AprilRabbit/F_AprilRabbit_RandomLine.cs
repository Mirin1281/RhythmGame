using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/ランダム線"), System.Serializable]
    public class F_AprilRabbit_RandomLine : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] int count = 96;
        [SerializeField] float waitLPB = 16;

        protected override async UniTask ExecuteAsync()
        {
            await WaitOnTiming();

            var rand = new System.Random();
            for (int i = 0; i < count; i++)
            {
                Line(rand.Next(-10, 10));
                await Wait(waitLPB);
            }
        }

        void Line(float x)
        {
            var line = Helper.GetLine();
            line.SetRot(90);
            line.SetPos(new Vector3(mirror.Conv(x), 0));
            line.FadeIn(0, 0.4f);
            line.FadeOut(0.3f);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "RandomLine";
        }
#endif
    }
}
