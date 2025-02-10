using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/最後のノーツ")]
    public class F_AprilRabbit_LastNote : CommandBase
    {
        [SerializeField] Mirror mirror;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();

            RegularNote note = Helper.GetRegularNote(RegularNoteType.Normal);
            Vector3 pos = new Vector3(0, 4);
            note.SetPos(pos);
            note.IsVerticalRange = true;
            Helper.PoolManager.SetSimultaneousSprite(note);
            float expectTime = new Lpb(4, 7).Time - Delta;
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, pos, expectTime));

            var line = Helper.GetLine();
            line.SetPos(pos);
            line.FadeIn(2f, 0.5f);
            var line2 = Helper.GetLine();
            line2.SetPos(pos);
            line2.FadeIn(2f, 0.1f);

            float time = 2.6f;
            var easing = new Easing(0, mirror.Conv(360 * 5), time, EaseType.OutCubic);
            WhileYield(time, t =>
            {
                note.SetRot(easing.Ease(t));
                line.SetRot(easing.Ease(t));
                line2.SetRot(easing.Ease(t) + 90);
            });

            await WaitOnTiming();

            var judgeLine = Helper.GetLine();
            judgeLine.SetWidth(100);
            judgeLine.SetPos(new Vector3(mirror.Conv(11), 4));
            judgeLine.SetAlpha(0.5f);

            var judgeLine2 = Helper.GetLine();
            judgeLine2.SetWidth(100);
            judgeLine2.SetPos(new Vector3(mirror.Conv(-11), 4));
            judgeLine2.SetAlpha(0.5f);

            float judgeTime = 0.6f;
            var judgeEasing = new Easing(mirror.Conv(90), mirror.Conv(270), judgeTime, EaseType.InQuad);
            WhileYield(judgeTime, t =>
            {
                judgeLine.SetRot(judgeEasing.Ease(t));
                judgeLine2.SetRot(judgeEasing.Ease(t));
            });

            CircleAsync(pos).Forget();

            await Wait(new Lpb(4));
            line.FadeOut(0);
            line2.FadeOut(0);
        }

        async UniTask CircleAsync(Vector3 startPos)
        {
            var circle = Helper.PoolManager.CirclePool.GetCircle();
            circle.SetPos(startPos);
            circle.SetAlpha(0.2f);
            float baseTime = CurrentTime - Delta;
            float t = 0f;
            while (circle.IsActive && t < 3f)
            {
                t = CurrentTime - baseTime;
                circle.SetScale(t.Ease(4f, 0f, new Lpb(4).Time, EaseType.InQuad));
                await Yield();
            }
            circle.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "LastNote";
        }
#endif
    }
}
