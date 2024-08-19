using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("ラベル"), System.Serializable]
    public class F_Label : Generator_Type1
    {
        [SerializeField] string summary;

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
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