using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/シャウト判定2"), System.Serializable]
    public class F_Quiela_CreateJudge2 : CommandBase
    {
        [SerializeField] Lpb time = new Lpb(1.6f);
        [SerializeField] Lpb lpbInterval = new Lpb(64);

        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();

            int interval = 12;
            for (int i = Mathf.RoundToInt(time / lpbInterval); i >= 0; i--)
            {
                float x = (i % interval - (interval - 1) / 2f) * 2.2f;
                float y = ((i * 2.7f) % interval - (interval - 1) / 2f) * 1.2f;
                Vector2 inputPos = (i % 4) switch
                {
                    0 => new Vector2(6, 0),
                    1 => new Vector2(2, 0),
                    2 => new Vector2(-2, 0),
                    3 => new Vector2(-6, 0),
                    _ => throw new ArithmeticException(),
                };
                EffectJudge(inputPos, new Vector2(x, y + 4), i % 2 == 0);
                await Wait(lpbInterval);
            }
        }

        /// <summary>
        /// 入力座標と被るような判定を生成します
        /// </summary>
        void EffectJudge(Vector2 inputPos, Vector2 judgePos, bool isMute = false)
        {
            /*var slide = Helper.GetRegularNote(RegularNoteType.Slide);
            slide.SetWidth(5f);
            slide.IsVerticalRange = true;
            var dir = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg + 90;
            slide.SetRot(dir);
            var judgeStatus = new NoteJudgeStatus(slide, judgePos, -Delta, expectType: NoteJudgeStatus.ExpectType.Static, isMute: isMute);
            Helper.NoteInput.AddExpect(judgeStatus);*/
            var dir = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg + 90;
            var judgeStatus = new NoteJudgeStatus(RegularNoteType.Slide, judgePos, -Delta, true, dir);
            judgeStatus.IsMute = isMute;
            Helper.NoteInput.AddExpect(judgeStatus);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Q_Judge2";
        }
#endif
    }
}