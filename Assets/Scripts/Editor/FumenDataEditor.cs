using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace NoteCreating.Editor
{
    [CustomEditor(typeof(FumenData))]
    public class FumenDataEditor : UnityEditor.Editor
    {
        static float _floatAField;
        static int popupIndex;
        static float _floatBField;

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

            using (new EditorGUILayout.HorizontalScope())
            {
                _floatAField = EditorGUILayout.FloatField(_floatAField);

                popupIndex = EditorGUILayout.Popup(popupIndex, new[] { "＋", "－" });

                _floatBField = EditorGUILayout.FloatField(_floatBField);
            }

            float result = CulcLPB(_floatAField, _floatBField, popupIndex == 0);

            EditorGUILayout.Space(5);

            EditorGUILayout.FloatField("Result", result);

            EditorGUI.indentLevel--;
        }

        float CulcLPB(float a, float b, bool addOrExcept)
        {
            if (a == 0)
            {
                if (b == 0)
                {
                    return 0;
                }
                else
                {
                    if (addOrExcept)
                    {
                        return b;
                    }
                    else
                    {
                        return -b;
                    }
                }
            }
            else if (b == 0)
            {
                return a;
            }

            if (addOrExcept)
            {
                return a * b / (a + b);
            }
            else
            {
                if (a == b) return 0;
                return a * b / (b - a);
            }
        }
    }
}