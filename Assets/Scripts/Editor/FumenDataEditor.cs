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

            if (GUILayout.Button("�t���[�`���[�g�G�f�B�^���J��"))
            {
                FumenEditorWindow.OpenWindow();
            }

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV�`���ŃG�N�X�|�[�g����"))
                {
                    FumenCSVIO.ExportFumenDataAsync(target as FumenData).Forget();
                }
                if (GUILayout.Button("CSV���C���|�[�g����"))
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
                    // ����̓G�f�B�^��œ��͂���󋵂Ȃ̂ŁA�G���[���o�������Ȃ����爬��
                }
            }
            EditorGUILayout.FloatField("Result", _resultField);

            EditorGUI.indentLevel--;
        }
    }
}