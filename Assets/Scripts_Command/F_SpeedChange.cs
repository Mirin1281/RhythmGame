using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◇速度変更"), System.Serializable]
    public class F_SpeedChange : CommandBase
    {
        [Header("通常を1として、全体のノーツスピードを変更します")]
        [Header("多用はおすすめしません")]
        [SerializeField] float speed = 1f;
        [SerializeField] Lpb easeLpb;
        [SerializeField] EaseType easeType = EaseType.Linear;

        protected override async UniTaskVoid ExecuteAsync()
        {
            var easing = new Easing(RhythmGameManager.SpeedBase, speed, easeLpb.Time, easeType);
            WhileYield(easeLpb.Time, t =>
            {
                RhythmGameManager.SpeedBase = easing.Ease(t);
            }, timing: PlayerLoopTiming.PreUpdate);
            await UniTask.CompletedTask;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }

        protected override string GetName()
        {
            return "速度変更";
        }

        protected override string GetSummary()
        {
            return $"To  {speed}   :   Lpb  {easeLpb.GetLpbValue()}";
        }
#endif
    }
}