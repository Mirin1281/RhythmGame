using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Sky.SkyNoteData))]
    public class SkyNoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var width = position.width;
            var x = position.x;

            position.x += width * 0.7f;
            var disableeProp = property.FindPropertyRelative("disable");
            EditorGUI.PropertyField(position, disableeProp, GUIContent.none);
            position.x = x;

            using(new EditorGUI.DisabledGroupScope(disableeProp.boolValue))
            {
                //EditorGUI.LabelField(position, "位置:");
                position.x += width * 0.04f;
                position.width = width * 0.25f;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("pos"), GUIContent.none);
                position.width = width * 0.25f;
            }
                
            position.x += width * 0.34f;
            EditorGUI.LabelField(position, "待ち:");
            position.x += width * 0.08f;
            position.width = width * 0.13f;
            var waitProp = property.FindPropertyRelative("wait");
            EditorGUI.PropertyField(position, waitProp, GUIContent.none);
            position.width = width * 0.25f;

            if(waitProp.floatValue == 0f)
            {
                using(new EditorGUI.DisabledGroupScope(disableeProp.boolValue))
                {
                    if(disableeProp.boolValue)
                    {
                        DrawBoxLayout(new Rect(19, position.y -2, width + 40, EditorGUIUtility.singleLineHeight + 4), Color.cyan);
                    }
                    else
                    {
                        DrawBoxLayout(new Rect(19, position.y -2, width + 40, 2 * (EditorGUIUtility.singleLineHeight + 4)), Color.yellow);
                    }
                }
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
}