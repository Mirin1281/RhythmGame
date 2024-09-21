using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆リザルト移行"), System.Serializable]
    public class F_Result : NoteGeneratorBase
    {
        protected override async UniTask GenerateAsync()
        {
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            var judge = GameObject.FindAnyObjectByType<Judgement>(FindObjectsInactive.Include);
            RhythmGameManager.Instance.Result = judge.Result;
            FadeLoadSceneManager.Instance.LoadScene(1, "Result");
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
