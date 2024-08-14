using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomEditor(typeof(GenerateData))]
    public class GenerateDataEditor : UnityEditor.Editor
    {
        GenerateData _target;

        public override void OnInspectorGUI()
        {
            if (_target == null)
            {
                _target = target as GenerateData;
            }
            if (_target != null)
            {
                DrawBoxLayout(_target.GetCommandColor());
            }
            
            var enabledProp = serializedObject.FindProperty("enable");
            EditorGUILayout.PropertyField(enabledProp);

            var beatTimingProp = serializedObject.FindProperty("beatTiming");
            EditorGUILayout.PropertyField(beatTimingProp);

            using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
            {
                var commandProp = serializedObject.FindProperty("generatable");
                EditorGUILayout.PropertyField(commandProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBoxLayout(Color color)
        {
            var originalColor = GUI.color;

            // Alpha�l�����������Ȃ��ƕ����������Ȃ��̂ŉ�����
            GUI.color = new Color(color.r, color.g, color.b, 0.2f);
            var style = new GUIStyle
            {
                normal =
            {
                background = Texture2D.whiteTexture
            }
            };
            GUI.Box(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 63f), string.Empty, style);

            GUI.color = originalColor;
        }
    }
}