using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/シャウト判定"), System.Serializable]
    public class F_Quiela_CreateJudge : CommandBase
    {
        [SerializeField] Vector2 startPos = new Vector2(0, 4);
        [SerializeField] int curveCount = 4;
        [SerializeField] float rotate = 3f;
        [SerializeField] float size = 0.8f;
        [SerializeField] Lpb time = new Lpb(1.6f);
        [SerializeField] Lpb lpbInterval = new Lpb(24);

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb);

            for (int i = 0; i < curveCount; i++)
            {
                float dir = i * 2 * Mathf.PI / curveCount;
                Vector2 inputPos = (i % 4) switch
                {
                    0 => new Vector2(0, 4),
                    1 => new Vector2(0, -4),
                    2 => new Vector2(-7, 0),
                    3 => new Vector2(7, 0),
                    _ => throw new System.ArithmeticException(),
                };
                CurveJudges(dir, inputPos, i != 0).Forget();
            }
        }

        async UniTaskVoid CurveJudges(float dir, Vector2 inputPos, bool isMute = true)
        {
            int count = Mathf.RoundToInt(time / lpbInterval);
            for (int i = 0; i < count; i++)
            {
                float d = i.Ease(0, rotate, count, EaseType.InQuad) - dir;
                Vector2 pos = size * i * new Vector2(Mathf.Cos(d), Mathf.Sin(d));
                EffectJudge(startPos + inputPos, startPos + pos, wideRange: false, isMute);
                await Wait(lpbInterval);
            }
        }

        /// <summary>
        /// 入力座標と被るような判定を生成します
        /// </summary>
        void EffectJudge(Vector2 inputPos, Vector2 judgePos, bool wideRange = false, bool isMute = false)
        {
            if (wideRange)
            {
                var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                slide.SetWidth(5f);
                slide.IsVerticalRange = true;
                var dir = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg + 90;
                slide.SetRot(dir);
                var judgeStatus = new NoteJudgeStatus(slide, judgePos, -Delta, expectType: NoteJudgeStatus.ExpectType.Static, isMute: isMute);
                Helper.NoteInput.AddExpect(judgeStatus);
            }
            else
            {
                var dir = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg + 90;
                var judgeStatus = new NoteJudgeStatus(RegularNoteType.Slide, judgePos, -Delta, true, dir);
                judgeStatus.IsMute = isMute;
                Helper.NoteInput.AddExpect(judgeStatus);
            }
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Q_Judge";
        }
#endif
    }
}