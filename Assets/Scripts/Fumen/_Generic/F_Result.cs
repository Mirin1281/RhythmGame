using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇リザルト移行", 100), System.Serializable]
    public class F_Result : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();
            var judge = GameObject.FindAnyObjectByType<Judgement>(FindObjectsInactive.Include);

            RhythmGameManager.Instance.Result = judge.Result;
            await FadeLoadSceneManager.Instance.LoadSceneAsync(1, "Result", 1, Color.white);
            RhythmGameManager.SpeedBase = 1f;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_UnNote;
        }
#endif
    }
}
