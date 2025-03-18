using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(TransformConverter))]
    public class TransformConverterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.LabelField(position, $"{nameof(NoteData)}のオプション1は0番目、オプション2は1番目以降に適用されます");
            //position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("transformConvertables"), new GUIContent("Convert Params"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("transformConvertables")) + EditorGUIUtility.singleLineHeight;
        }
    }
}