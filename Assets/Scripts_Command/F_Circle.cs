using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆円を制御"), System.Serializable]
    public class F_Circle : CommandBase
    {
        [System.Serializable]
        struct CircleData
        {
            [SerializeField] Lpb wait;

            [SerializeField] Vector2 pos;

            [Header("Size")]
            [SerializeField] EasingStatus_Lpb sizeStatus;

            [Header("Alpha")]
            [SerializeField] EasingStatus_Lpb alphaStatus;

            [Header("Width")]
            [SerializeField] EasingStatus_Lpb widthStatus;

            public readonly Lpb Wait => wait;
            public readonly Vector2 Pos => pos;
            public readonly EasingStatus_Lpb SizeStatus => sizeStatus;
            public readonly EasingStatus_Lpb AlphaStatus => alphaStatus;
            public readonly EasingStatus_Lpb WidthStatus => widthStatus;

            public CircleData(bool _)
            {
                wait = Lpb.Zero;
                pos = Vector2.zero;
                sizeStatus = new(1, 1);
                widthStatus = new(0.1f, 0.1f);
                alphaStatus = new(0.5f, 0.5f);
            }
        }

        [SerializeField] Mirror mirror;
        [SerializeField] Vector2 basePos = new Vector2(0, 4);
        [SerializeField] CircleData[] circleDatas = new CircleData[] { new CircleData(true) };

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb, Delta);
            foreach (var data in circleDatas)
            {
                delta = await Wait(data.Wait, delta);
                CreateCircle(data, delta).Forget();
            }
        }

        async UniTaskVoid CreateCircle(CircleData data, float delta)
        {
            var circle = Helper.GetCircle();
            circle.SetPos(mirror.Conv(data.Pos + basePos));

            var sizeEasing = new Easing(data.SizeStatus);
            sizeEasing.EaseAsync(Helper.Token, delta, t =>
            {
                circle.SetScale(t);
            }).Forget();

            var widthEasing = new Easing(data.WidthStatus);
            widthEasing.EaseAsync(Helper.Token, delta, t =>
            {
                circle.SetWidth(t);
            }).Forget();

            var alphaEasing = new Easing(data.AlphaStatus);
            await alphaEasing.EaseAsync(Helper.Token, delta, t =>
            {
                circle.SetAlpha(t);
            });
            circle.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Circle";
        }

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }
#endif
    }
}