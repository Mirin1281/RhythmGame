using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇ラベル"), System.Serializable]
    public class F_Label : CommandBase
    {
        [SerializeField] string summary;

        protected override UniTask ExecuteAsync()
        {
            return UniTask.CompletedTask;
        }

        protected override Color GetCommandColor()
        {
            return new Color(0.5f, 0.9f, 0.7f);
        }

        protected override string GetSummary()
        {
            return summary;
        }
    }
}