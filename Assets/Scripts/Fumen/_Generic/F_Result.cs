using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◇リザルト移行", 5), System.Serializable]
    public class F_Result : NoteGeneratorBase
    {
        protected override async UniTask GenerateAsync()
        {
            await WaitOnTiming();
            var judge = GameObject.FindAnyObjectByType<Judgement>(FindObjectsInactive.Include);

            RhythmGameManager.Instance.Result = judge.Result;
            await FadeLoadSceneManager.Instance.LoadSceneAsync(1, "Result", 1, Color.white);
            RhythmGameManager.SpeedBase = 1f;
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
