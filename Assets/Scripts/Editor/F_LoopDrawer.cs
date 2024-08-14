using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Loop.NoteData))]
    public class F_LoopNoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var width = position.width;

            var isInvalidProp = property.FindPropertyRelative("isInvalid");
            using(new EditorGUI.DisabledScope(isInvalidProp.boolValue))
            {
                position.width = width * 0.5f;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("x"), GUIContent.none);
            }

            position.x += width * 0.55f;
            EditorGUI.PropertyField(position, isInvalidProp, GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(F_Loop))]
    public class F_LoopDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isInverse"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("summary"));

            position.y += EditorGUIUtility.singleLineHeight;
            var noteTypeProp = property.FindPropertyRelative("noteType");
            EditorGUI.PropertyField(position, noteTypeProp);
            NoteType noteType = (NoteType)noteTypeProp.enumValueIndex;

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("lpb"));

            if(noteType == NoteType.Hold)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("length"));
            }

            position.y += EditorGUIUtility.singleLineHeight;
            var noteDatasProp = property.FindPropertyRelative("noteDatas");
            EditorGUI.PropertyField(position, noteDatasProp);

            if(noteDatasProp.isExpanded == false) return;

            // こうすると領域が確保されてスクロールバーが出現する
            GUILayoutUtility.GetRect(0, 180 + noteDatasProp.arraySize * EditorGUIUtility.singleLineHeight);

            var endProperty = noteDatasProp.GetEndProperty();
            noteDatasProp.NextVisible(true);
            position.y += 6;
            int i = 0;
            while (noteDatasProp.NextVisible(false))
            {
                position.y += 20;
                i++;
                if (SerializedProperty.EqualContents(noteDatasProp, endProperty)) break;
                if (noteDatasProp.propertyType == SerializedPropertyType.ArraySize
                 || i % 2 == 0) continue;
                DrawBoxLayout(position, Color.white);
            }
        }

        void DrawBoxLayout(Rect position, Color color)
        {
            var originalColor = GUI.color;

            // Alpha値を小さくしないと文字が見えないので下げる
            GUI.color = new Color(color.r, color.g, color.b, 0.1f);
            var style = new GUIStyle
            {
                normal =
                {
                    background = Texture2D.whiteTexture
                }
            };
            GUI.Box(new Rect(position.x + 16, position.y, position.width, position.height), string.Empty, style);

            GUI.color = originalColor;
        }
    }
}