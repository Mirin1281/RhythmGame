using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇ラベル"), System.Serializable]
    public class F_Label : CommandBase
    {
        protected override UniTaskVoid ExecuteAsync()
        {
            return default;
        }

#if UNITY_EDITOR

        enum LabelType
        {
            [InspectorName("なし")] None,
            [InspectorName("ビルドアップ")] BuildUp,
            [InspectorName("サビ")] Drop,
            [InspectorName("ヴァース")] Verse,
            [InspectorName("アウトロ")] Out,
        }

        [Space(20)]
        [SerializeField] LabelType labelType = LabelType.None;
        [SerializeField] string summary;

        protected override Color GetCommandColor()
        {
            return new Color(0.5f, 0.9f, 0.7f);
        }

        protected override string GetSummary()
        {
            string type = labelType switch
            {
                LabelType.None => string.Empty,
                LabelType.BuildUp => "ビルドアップ",
                LabelType.Drop => "サビ",
                LabelType.Verse => "ヴァース",
                LabelType.Out => "アウトロ",
                _ => string.Empty
            };
            string join;
            if (string.IsNullOrEmpty(summary) || labelType == LabelType.None)
            {
                join = string.Empty;
            }
            else
            {
                join = " : ";
            }
            return type + join + summary;
        }
#endif
    }
}