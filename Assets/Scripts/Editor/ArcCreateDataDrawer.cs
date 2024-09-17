using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(ArcCreateData))]
    public class ArcCreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property, EditorGUIUtility.singleLineHeight);
            var width = hel.StartWidth;

            hel.PropertyField("pos", false);

            hel.SetY();
            hel.PropertyField("vertexMode");

            hel.SetY();
            hel.SetWidth(width / 2f);
            var disableProp = hel.PropertyField("isJudgeDisable");

            hel.SetX(width / 2f);
            hel.PropertyField("isDuplicated");

            using (new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                hel.SetY();
                var w = width / 4f;
                hel.SetWidth(w);

                hel.SetX(0);
                hel.LabelField("手前");

                hel.SetX(w - 30);
                hel.PropertyField("behindJudgeRange", false);

                hel.SetX(w * 2f);
                hel.LabelField("奥");

                hel.SetX(w * 3f - 30);
                hel.PropertyField("aheadJudgeRange", false);
            }

            position.x = hel.SetX(-10);
            position.y = hel.SetY(5);
            position.width = width + 15;
            position.height = 1;
            EditorGUI.DrawRect(position, new Color(0.7f, 0.7f, 0.7f));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 4.5f * EditorGUIUtility.singleLineHeight;
        }
    }
}
