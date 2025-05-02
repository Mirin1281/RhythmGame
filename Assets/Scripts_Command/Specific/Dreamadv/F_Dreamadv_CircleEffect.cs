using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Dreamadv/上昇する円"), System.Serializable]
    public class F_Dreamadv_CircleEffect : CommandBase
    {
        [Space(15)]
        [SerializeField] int count = 32;
        [SerializeField] Lpb wait = new Lpb(4);
        [Space(15)]
        [SerializeField] Vector2 scaleRange = new Vector2(1f, 5f);
        [SerializeField] Vector2 widthRange = new Vector2(0.3f, 0.1f);
        [SerializeField] Vector2 alphaRange = new Vector2(0.3f, 0.1f);
        [SerializeField] Vector2 speedRange = new Vector2(10f, 20f);
        [Space(15)]
        [SerializeField] Lpb lifeLpb = new Lpb(1);
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] int seed = 57682736;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb);

            var random = Unity.Mathematics.Random.CreateFromIndex((uint)seed);

            for (int i = 0; i < count; i++)
            {
                CreateCircle(
                    random.NextFloat(scaleRange.x, scaleRange.y),
                    random.NextFloat(widthRange.x, widthRange.y),
                    random.NextFloat(alphaRange.x, alphaRange.y),
                    random.NextFloat(-12, 12),
                    random.NextFloat(speedRange.x, speedRange.y),
                    delta).Forget();
                delta = await Wait(wait, delta);
            }
        }

        async UniTaskVoid CreateCircle(float scale, float width, float alpha, float x, float speed, float delta)
        {
            var circle = Helper.GetCircle();

            var scaleEasing = new Easing(scale, 0, lifeLpb.Time, easeType);
            var widthEasing = new Easing(width, 0, lifeLpb.Time, easeType);
            var alphaEasing = new Easing(alpha, 0, lifeLpb.Time, easeType);

            float baseTime = CurrentTime - delta;
            while (true)
            {
                float t = CurrentTime - baseTime;
                circle.SetScale(scaleEasing.Ease(t));
                circle.SetWidth(widthEasing.Ease(t));
                circle.SetAlpha(alphaEasing.Ease(t));
                circle.SetPos(new Vector3(x, -6 + speed * t));
                if (t >= lifeLpb.Time) break;
                await Yield();
            }
            circle.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "CircleEffect";
        }
#endif
    }
}