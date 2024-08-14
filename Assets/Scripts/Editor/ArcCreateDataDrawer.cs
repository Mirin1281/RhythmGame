using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Cysharp.Threading.Tasks;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(ArcCreateData))]
    public class ArcCreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var x = position.x;
            var width = position.width;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("pos"), GUIContent.none);
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("vertexMode"));

            position.y += EditorGUIUtility.singleLineHeight;
            var disableProp = property.FindPropertyRelative("isJudgeDisable");
            EditorGUI.PropertyField(position, disableProp);

            using (new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                position.y += EditorGUIUtility.singleLineHeight;

                position.width = position.width / 4f;
                EditorGUI.LabelField(position, "手前");
                var tmpX = position.x;
                position.x += position.width - 30;
                var tmpWidth = position.width;
                position.width += 30;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("behindJudgeRange"), GUIContent.none);

                position.width = tmpWidth;
                position.x = tmpX + position.width * 2f + 30;
                EditorGUI.LabelField(position, "奥");
                position.x += position.width - 60;
                position.width += 30;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("aheadJudgeRange"), GUIContent.none);
            }

            position.x = x - 10;
            position.y += EditorGUIUtility.singleLineHeight + 5;
            position.width = width + 15;
            position.height = 1;
            EditorGUI.DrawRect(position, Color.white);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 4.5f * EditorGUIUtility.singleLineHeight;
        }
    }
}
