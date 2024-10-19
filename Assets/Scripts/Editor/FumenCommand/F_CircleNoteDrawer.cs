using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_CircleNote.NoteData))]
    public class F_CircleNoteDrawer : PropertyDrawer
    {
        static readonly float Height = 18;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property, Height + 2);
            float width = hel.StartWidth;
            float labelWidth = width * 0.2f;
            float boxWidth = width * 0.12f;

            hel.SetXAsWidth(0.8f);
            var disabledProp = hel.PropertyField(boxWidth, "disabled", false);

            using(new EditorGUI.DisabledGroupScope(disabledProp.boolValue))
            {
                hel.SetXAsWidth(0f);
                hel.LabelField(labelWidth, "Pos");

                hel.SetXAsWidth(0.1f);
                hel.PropertyField(boxWidth, "x", false);
                hel.SetXAsWidth(0.25f);
                hel.PropertyField(boxWidth, "y", false);
            }

            hel.SetXAsWidth(0.45f);
            hel.LabelField(labelWidth, "Wait");
            
            hel.SetXAsWidth(0.55f);
            var waitProp = hel.PropertyField(boxWidth, "wait", false);


            if(waitProp.floatValue == 0f)
            {
                DrawBoxLayout(new Rect(19, position.y - 2, width + 40, 2f * (Height + 4)), Color.yellow);
            }
        }

        void DrawBoxLayout(Rect position, Color color)
        {
            var originalColor = GUI.color;

            // Alpha値を小さくしないと文字が見えないので下げる
            GUI.color = new Color(color.r, color.g, color.b, 0.08f);
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => Height;
    }

    /*[CustomPropertyDrawer(typeof(F_CircleNote))]
    public class F_Generic2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isInverse"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("speedRate"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isSpeedChangable"));

            position.y += EditorGUIUtility.singleLineHeight;
            var parentProp = property.FindPropertyRelative("parentGeneratable");
            EditorGUI.PropertyField(new Rect(position.x - 15, position.y, position.width + 15, position.height), parentProp);


            position.y += EditorGUI.GetPropertyHeight(parentProp);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isCheckSimultaneous"));

            position.y += EditorGUIUtility.singleLineHeight;
            var noteDatasProp = property.FindPropertyRelative("noteDatas");
            EditorGUI.PropertyField(position, noteDatasProp);

            if(noteDatasProp.isExpanded == false) return;

            GUILayoutUtility.GetRect(0, 140 + noteDatasProp.arraySize * (18 + 2));

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
    }*/
}