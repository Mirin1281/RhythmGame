using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇ラベル")]
    public class F_Label : CommandBase
    {
        protected override UniTaskVoid ExecuteAsync()
        {
            return default;
        }

#if UNITY_EDITOR

        [SerializeField] string summary;

        protected override Color GetCommandColor()
        {
            return new Color(0.5f, 0.9f, 0.7f);
        }

        protected override string GetSummary()
        {
            return summary;
        }
#endif
    }
}