using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace NoteCreating.Editor
{
    [CustomEditor(typeof(FumenData))]
    public class FumenDataEditor : UnityEditor.Editor
    {
        static string _stringField;
        static float _resultField;

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


            EditorGUILayout.Space(20);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("LPB Culculate");
            EditorGUILayout.Space(5);

            _stringField = EditorGUILayout.TextField(_stringField);
            EditorGUILayout.Space(5);

            if (string.IsNullOrEmpty(_stringField))
            {
                _resultField = 0;
            }
            else
            {
                try
                {
                    var formura = Calc.Calc.Analyze(_stringField);
                    if (formura.IsCalcable)
                    {
                        _resultField = formura.Calc(null);
                    }
                }
                catch
                {
                    // 今回はエディタ上で入力する状況なので、エラーを出したくないから握り
                }
            }
            EditorGUILayout.FloatField("Result", _resultField);

            EditorGUI.indentLevel--;
        }
    }
}