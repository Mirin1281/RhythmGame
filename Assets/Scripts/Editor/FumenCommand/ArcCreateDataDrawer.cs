using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(ArcCreateData))]
    public class ArcCreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property);
            var width = h.GetWidth();

            h.PropertyField(width / 2f, "pos", false);

            h.SetX(width / 1.8f);
            h.PropertyField(width / 2.2f, "vertexMode", false);

            h.SetY();
            h.SetWidth(width / 2f);
            var disableProp = h.PropertyField("isJudgeDisable");

            using (new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                h.SetX(width / 2f);
                h.PropertyField("isDuplicated");

                h.SetY();
                var w = width / 4f;
                h.SetWidth(w);

                h.SetX(0);
                h.LabelField("手前");

                h.SetX(w - 30);
                h.PropertyField("behindJudgeRange", false);

                h.SetX(w * 2f);
                h.LabelField("奥");

                h.SetX(w * 3f - 30);
                h.PropertyField("aheadJudgeRange", false);
            }
            h.SetY(10);
            h.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 4f * 20;
        }
    }
}
