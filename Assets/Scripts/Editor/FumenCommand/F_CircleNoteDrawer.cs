using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_CircleNote.NoteData))]
    public class F_CircleNoteDrawer : PropertyDrawer
    {
        const float Height = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property, Height);
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
                DrawBoxLayout(new Rect(20, position.y, width + 40, 2f * (Height + 2)), Color.yellow);
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
}