using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇速度変更"), System.Serializable]
    public class F_SpeedChange : CommandBase
    {
        [Header("通常を1として、全体のノーツスピードを変更します")]
        [SerializeField] float speed = 1f;
        [SerializeField, Tooltip("何秒かけて変速するか設定します")] Lpb easeLpb;
        [SerializeField] EaseType easeType = EaseType.Linear;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float easeTime = easeLpb.Time;
            var easing = new Easing(RhythmGameManager.SpeedBase, speed, easeTime, easeTime == 0 ? EaseType.End : easeType);
            float baseTime = CurrentTime - Delta;
            float t = 0f;
            while (t < easeTime)
            {
                t = CurrentTime - baseTime;
                RhythmGameManager.SpeedBase = easing.Ease(t);
                await Yield(timing: PlayerLoopTiming.EarlyUpdate);
            }
            RhythmGameManager.SpeedBase = easing.Ease(easeTime);
            await UniTask.CompletedTask;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_UnNote;
        }

        protected override string GetName()
        {
            return "速度変更";
        }

        protected override string GetSummary()
        {
            return $"To  {speed}   :   EaseTime  {easeLpb}";
        }
#endif
    }
}