using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(Mirror))]
    public class MirrorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("enabled"), new GUIContent("Mirror"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}