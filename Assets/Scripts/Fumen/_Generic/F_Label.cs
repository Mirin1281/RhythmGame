using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("◇ラベル"), System.Serializable]
    public class F_Label : NoteGeneratorBase
    {
        [SerializeField] string summary;

        protected override UniTask GenerateAsync()
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

        public override string CSVContent1
        {
            get => summary;
            set => summary = value;
        }
    }
}