using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace NoteGenerating.Editor
{
    [CustomEditor(typeof(FumenData))]
    public class FumenDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                FumenEditorWindow.OpenWindow();
            }

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV形式でエクスポートする"))
                {
                    FumenCSVIO.ExportFumenDataAsync(target as FumenData).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
                {
                    FumenCSVIO.ImportFumenDataAsync(target as FumenData).Forget();
                }
            }
        }
    }
}