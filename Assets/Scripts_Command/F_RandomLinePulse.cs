using UnityEngine;
using Cysharp.Threading.Tasks;
using Random = System.Random;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◇ランダム線"), System.Serializable]
    public class F_RandomLinePulse : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] Lpb wholeLpb = new Lpb(1f);
        [SerializeField] Lpb interval = new Lpb(64);
        [Space(20)]
        [SerializeField] Vector2 xRange = new Vector2(-15f, 15f);
        [SerializeField] Vector2 yRange = new Vector2(-8f, 8f);
        [SerializeField] Vector2 rotRange = new Vector2(0, 360);
        [SerializeField] Vector2 alphaRange = new Vector2(0, 0.2f);
        [SerializeField] Vector2 widthRange = new Vector2(0.1f, 0.1f);
        [Space(20)]
        [SerializeField] int seed = 200;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb);

            Random randPos = new Random(seed);
            Random randRot = new Random(seed + 1);
            Random randAlpha = new Random(seed + 2);
            Random randWidth = new Random(seed + 3);
            int count = Mathf.RoundToInt(wholeLpb / interval);
            for (int i = 0; i < count; i++)
            {
                CreateLine(
                    mirror.Conv(randPos.GetVector2(new Vector2(xRange.x, yRange.x), new Vector2(xRange.y, yRange.y))),
                    mirror.Conv(randRot.GetFloat(rotRange.x, rotRange.y)),
                    randAlpha.GetFloat(alphaRange.x, alphaRange.y),
                    randWidth.GetFloat(widthRange.x, widthRange.y), delta).Forget();
                delta = await Wait(interval, delta);
            }

            async UniTaskVoid CreateLine(Vector3 pos, float rot, float alpha, float width, float delta)
            {
                var line = Helper.GetLine();
                line.SetPos(pos);
                line.SetRot(rot);
                line.SetAlpha(alpha);
                line.SetHeight(width);
                await WhileYieldAsync(0.3f, t =>
                {
                    line.SetAlpha(alpha - t);
                }, delta);
                line.SetActive(false);
            }
        }
#if UNITY_EDITOR
        protected override string GetName()
        {
            return "RandomLine";
        }

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Line;
        }

        protected override string GetSummary()
        {
            return mirror.GetStatusText();
        }
#endif
    }
}