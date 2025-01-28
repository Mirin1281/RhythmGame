using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇速度変更"), System.Serializable]
    public class F_SpeedChange : CommandBase
    {
        [Header("通常を1として、全体のノーツスピードを変更します")]
        [SerializeField] float speed = 1f;
        [SerializeField, Min(0), Tooltip("何秒かけて変速するか設定します")] float easeTimeLPB;
        [SerializeField] EaseType easeType = EaseType.Linear;

        protected override async UniTask ExecuteAsync()
        {
            float easeTime = Helper.GetTimeInterval(easeTimeLPB);
            var easing = new Easing(RhythmGameManager.SpeedBase, speed, easeTime, easeType);
            /*float baseTime = CurrentTime - Delta;
            float t = 0f;
            while (t < easeTime)
            {
                t = CurrentTime - baseTime;
                SetSpeed(easing.Ease(t));
                await UniTask.Yield(Helper.Token);
            }
            SetSpeed(easing.Ease(easeTime));*/

            WhileYield(easeTime, t =>
            {
                RhythmGameManager.SpeedBase = easing.Ease(t);
            });
            await UniTask.CompletedTask;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetName()
        {
            return "速度変更";
        }

        protected override string GetSummary()
        {
            return $"To  {speed}   :   EaseTime  {easeTimeLPB}";
        }
#endif
    }
}