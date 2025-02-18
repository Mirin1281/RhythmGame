using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆判定線", -50), System.Serializable]
    public class F_JudgeLine : CommandBase, INotSkipCommand
    {
        [SerializeField] Mirror mirror;

        [Space(20)]
        [SerializeField] float alpha = 1f;

        [Space(20)]
        [SerializeField] Lpb fadeInLpb = new Lpb(4);
        [SerializeField] Lpb lifeLpb;
        [SerializeField] Lpb fadeOutLpb = new Lpb(4);

        [Space(20)]
        [SerializeField] float deg;

        [SerializeField] Vector2 pos;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb);
            Line line = Helper.GetLine();
            line.SetRot(deg);
            line.SetPos(pos);
            line.SetAlpha(0);
            line.FadeAlphaAsync(alpha, fadeInLpb.Time).Forget();
            await Wait(lifeLpb - fadeOutLpb);
            line.FadeAlphaAsync(0, fadeOutLpb.Time).Forget();
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
            string status = $"Lpb: {lifeLpb.GetLpbValue()}";
            return status + mirror.GetStatusText();
        }
#endif
    }
}