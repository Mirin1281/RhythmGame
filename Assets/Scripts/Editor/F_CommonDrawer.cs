using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Common.NoteData))]
    public class F_CommonNoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var width = position.width;

            position.width = width * 0.25f;
            var typeProp = property.FindPropertyRelative("type");
            EditorGUI.PropertyField(position, typeProp, GUIContent.none);
            var type = (F_Common.CreateNoteType)typeProp.enumValueIndex;

            if(type == F_Common.CreateNoteType._None)
            {
                DrawBoxLayout(new Rect(19, position.y - 2, width + 40, EditorGUIUtility.singleLineHeight + 4), Color.cyan);
                position.x += width * 0.525f;
                EditorGUI.LabelField(position, "待ち:");
                position.x += width * 0.08f;
                position.width = width * 0.13f;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("wait"), GUIContent.none);
                position.width = width * 0.25f;
                return;
            }

            position.x += width * 0.275f;
            EditorGUI.LabelField(position, "位置:");
            position.x += width * 0.08f;
            position.width = width * 0.13f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("x"), GUIContent.none);
            position.width = width * 0.25f;
            
            position.x += width * 0.17f;
            EditorGUI.LabelField(position, "待ち:");
            position.x += width * 0.08f;
            position.width = width * 0.13f;
            var waitProp = property.FindPropertyRelative("wait");
            EditorGUI.PropertyField(position, waitProp, GUIContent.none);
            position.width = width * 0.25f;

            if(waitProp.floatValue == 0f)
            {
                DrawBoxLayout(new Rect(19, position.y -2, width + 40, 2 * (EditorGUIUtility.singleLineHeight + 4)), Color.yellow);
            }

            if(type == F_Common.CreateNoteType.Hold)
            {
                position.x += width * 0.17f;
                EditorGUI.LabelField(position, "長さ:");
                position.x += width * 0.08f;
                position.width = width * 0.13f;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("length"), GUIContent.none);
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
            GUI.Box(position, string.Empty, style);

            GUI.color = originalColor;
        }
    }

    [CustomPropertyDrawer(typeof(F_Common))]
    public class F_CommonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isInverse"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("summary"));

            position.y += EditorGUIUtility.singleLineHeight;
            var noteDatasProp = property.FindPropertyRelative("noteDatas");
            EditorGUI.PropertyField(position, noteDatasProp);

            if(noteDatasProp.isExpanded == false) return;

            GUILayoutUtility.GetRect(0, 140 + noteDatasProp.arraySize * EditorGUIUtility.singleLineHeight);

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
            }
        }
    }
}