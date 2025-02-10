using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇判定の発生"), System.Serializable]
    public class F_JudgeCreate : CommandBase
    {
        [SerializeField] RegularNoteType judgeNoteType = RegularNoteType.Slide;
        [SerializeField] Vector2 pos;
        [SerializeField] bool isVerticalRange;
        [SerializeField] float rotate;
        [SerializeField] Lpb delayAfter;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float time = MoveTime + delayAfter.Time;
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(judgeNoteType, pos, time - Delta, isVerticalRange, rotate));
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 入力座標と被るような判定を生成します
        /// </summary>
        void EffectJudge(Vector2 inputPos, Vector2 judgePos, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            var dir = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg;
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(RegularNoteType.Slide, judgePos, -delta, true, dir));
        }

#if UNITY_EDITOR

        /*[SerializeField] string summary;

        protected override Color GetCommandColor()
        {
            return new Color(0.5f, 0.9f, 0.7f);
        }

        protected override string GetSummary()
        {
            return summary;
        }*/
#endif
    }
}