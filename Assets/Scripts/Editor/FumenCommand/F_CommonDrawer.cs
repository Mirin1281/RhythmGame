using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Generic2D.NoteData))]
    public class F_Generic2D_NoteDataDrawer : PropertyDrawer
    {
        static readonly float Height = 18;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property, Height + 2);
            var width = hel.StartWidth;
            hel.SetWidth(width * 0.18f);

            var typeProp = hel.PropertyField("type", false);
            var type = (F_Generic2D.CreateNoteType)typeProp.enumValueIndex;
            hel.SetWidth(width * 0.12f);

            if(type == F_Generic2D.CreateNoteType._None)
            {
                float x = hel.SetX(width / 5f * 2f);
                hel.LabelField("待:");
                hel.SetX(x - 30f);
                hel.PropertyField("wait", false);

                DrawBoxLayout(new Rect(19, position.y - 2, width + 40, Height + 4), Color.cyan);
                return;
            }

            float x1 = hel.SetX(width / 5f);
            hel.LabelField("X:");
            hel.SetX(x1 - 30f);
            hel.PropertyField("x", false);
            
            float x2 = hel.SetX(width / 5f * 2f);
            hel.LabelField("待:");
            hel.SetX(x2 - 30f);
            var waitProp = hel.PropertyField("wait", false);

            float x3 = hel.SetX(width / 5f * 3f);
            hel.LabelField("幅:");
            hel.SetX(x3 - 30f);
            hel.PropertyField("width", false);

            if(type == F_Generic2D.CreateNoteType.Hold)
            {
                float x4 = hel.SetX(width / 5f * 4f);
                hel.LabelField("長:");
                hel.SetX(x4 - 30f);
                hel.PropertyField("length", false);
            }

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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => Height;
    }

    [CustomPropertyDrawer(typeof(F_Generic2D))]
    public class F_Generic2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isInverse"));

            position.y += EditorGUIUtility.singleLineHeight;
            //EditorGUI.PropertyField(position, property.FindPropertyRelative("speedRate"));

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
    }
}