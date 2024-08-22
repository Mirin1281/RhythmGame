using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_CameraMove))]
    public class F_CameraMoveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var x = position.x;
            var w = position.width;
            position.y += GetHeight();

            EditorGUI.LabelField(position, "Pos");
            position.x = x + 60;
            position.width = w - 60;
            var isPosMove_p = property.FindPropertyRelative("isPosMove");
            EditorGUI.PropertyField(position, isPosMove_p, GUIContent.none);
            position.x = x + 120;
            position.width = w - 120;
            using (new EditorGUI.DisabledGroupScope(isPosMove_p.boolValue == false))
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("pos"), GUIContent.none);
            }

            position.y += GetHeight();
            position.x = x;
            position.width = w;

            EditorGUI.LabelField(position, "Rotate");
            position.x = x + 60;
            position.width = w - 60;
            var isRotateMove_p = property.FindPropertyRelative("isRotateMove");
            EditorGUI.PropertyField(position, isRotateMove_p, GUIContent.none);
            position.x = x + 120;
            position.width = w - 120;
            using (new EditorGUI.DisabledGroupScope(isRotateMove_p.boolValue == false))
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("rotate"), GUIContent.none);
                position.y += GetHeight();
                EditorGUI.LabelField(position, "isRotateClamp");
                position.x = x + 220;
                position.y -= 4;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("isRotateClamp"), GUIContent.none);
                position.y += 4;
            }

            position.x = x;
            position.width = w;

            position.y += GetHeight();
            EditorGUI.PropertyField(position, property.FindPropertyRelative("time"));
            position.y += GetHeight();
            EditorGUI.PropertyField(position, property.FindPropertyRelative("easeType"));
            position.y += GetHeight();
            EditorGUI.PropertyField(position, property.FindPropertyRelative("moveType"));
            position.y += GetHeight();
            EditorGUI.PropertyField(position, property.FindPropertyRelative("delay"));
        }

        float GetHeight() => 24f;
    }
}
