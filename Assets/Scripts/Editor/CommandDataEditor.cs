using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomEditor(typeof(CommandData))]
    public class CommandDataEditor : UnityEditor.Editor
    {
        CommandData _target;

        public override void OnInspectorGUI()
        {
            if (_target == null)
            {
                _target = target as CommandData;
            }

            if (_target != null)
            {
                DrawBoxLayout(_target.GetCommandColor());
            }

            var enabledProp = serializedObject.FindProperty("enable");
            EditorGUILayout.PropertyField(enabledProp);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beatTiming"));
            using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("command"));
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