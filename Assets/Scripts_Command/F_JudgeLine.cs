using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◆判定線", -50), System.Serializable]
    public class F_JudgeLine : CommandBase, INotSkipCommand
    {
        [Space(20)]
        [SerializeField] Mirror mirror;

        [Space(10)]
        [SerializeField] float alpha = 1f;

        [Space(10)]
        [SerializeField] Lpb fadeInLpb = new Lpb(4);
        [Space(10)]
        [SerializeField] Lpb lifeLpb = new Lpb(4);
        [SerializeField] float lifeCount = 32;
        [Space(10)]
        [SerializeField] Lpb fadeOutLpb = new Lpb(4);

        [Space(20)]
        [SerializeField] float deg;
        [SerializeField] Vector2 pos;

        protected override async UniTaskVoid ExecuteAsync()
        {
            if (Delta > lifeLpb.Time * lifeCount + MoveLpb.Time) return;
            await Wait(MoveLpb);
            Line line = Helper.GetLine();
            line.SetPos(pos);
            line.SetRot(deg);

            line.SetAlpha(0);
            line.FadeAlphaAsync(alpha, fadeInLpb.Time, delta: Delta).Forget();
            await Wait(lifeLpb * lifeCount - fadeOutLpb);
            line.FadeAlphaAsync(0, fadeOutLpb.Time, delta: Delta).Forget();
        }


#if UNITY_EDITOR

        protected override string GetName()
        {
            return "判定線";
        }

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Line;
        }

        protected override string GetSummary()
        {
            string status = $"Length: {lifeLpb / new Lpb(4) * lifeCount}";
            return status + mirror.GetStatusText();
        }

        public override void OnPeriod()
        {
            var previewer = CommandEditorUtility.GetPreviewer();
            Line line = Helper.GetLine();
            line.SetRot(deg);
            line.SetPos(pos);
            previewer.SetChild(line);
        }
#endif
    }
}