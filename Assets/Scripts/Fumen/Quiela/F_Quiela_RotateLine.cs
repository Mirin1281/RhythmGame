using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/サビ判定線回転"), System.Serializable]
    public class F_Quiela_RotateLine : CommandBase
    {
        [SerializeField] Vector2 startPos = new Vector2(0, 10);
        [SerializeField] int count = 16;
        [SerializeField] Lpb lpb = new Lpb(1.25f);
        [SerializeField] float size = 8f;
        [SerializeField] float rotSpeed = 8f;
        [SerializeField] float maxAlpha = 0.1f;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();

            var rotEasing = new Easing(0, rotSpeed, lpb.Time, EaseType.InOutQuad);
            var alphaEasing = new Easing(0, maxAlpha, lpb.Time / 2, EaseType.OutQuad);
            for (int i = 0; i < count; i++)
            {
                float dir = i * 2 * Mathf.PI / count;
                Line(dir, rotEasing, alphaEasing).Forget();
            }
        }

        async UniTaskVoid Line(float dir, Easing rotEasing, Easing alphaEasing)
        {
            var line = Helper.GetLine();
            await WhileYieldAsync(lpb.Time, t =>
            {
                float d = rotEasing.Ease(t) - dir;
                var pos = t * size * new Vector2(Mathf.Cos(d), Mathf.Sin(d));
                line.SetPos(startPos + pos);
                line.SetRot(d * Mathf.Rad2Deg + 90);
                line.SetAlpha(alphaEasing.Ease(t));
            });
            line.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Q_RotLine";
        }
#endif
    }
}