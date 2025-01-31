using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇判定の発生"), System.Serializable]
    public class F_JudgeCreate : CommandBase
    {
        [SerializeField] RegularNoteType judgeNoteType;
        [SerializeField] Vector2 pos;
        [SerializeField] float delayAfter;

        protected override async UniTask ExecuteAsync()
        {
            await WaitOnTiming();
            await Wait(delayAfter);
            var invisibleNote = Helper.GetRegularNote(judgeNoteType);
            invisibleNote.SetRendererEnabled(false);
            Helper.NoteInput.AddExpect(invisibleNote, pos, -Delta, expectType: NoteJudgeStatus.ExpectType.Static);
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