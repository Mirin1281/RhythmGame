using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◇リザルト移行", 100), System.Serializable]
    public class F_Result : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb);
            var judge = GameObject.FindAnyObjectByType<Judgement>(FindObjectsInactive.Include);

            RhythmGameManager.Instance.Result = judge.Result;
            await FadeLoadSceneManager.Instance.LoadSceneAsync(1, "Result", 1);
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }
#endif
    }
}
