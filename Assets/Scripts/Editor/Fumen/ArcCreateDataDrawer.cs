using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(ArcCreateData))]
    public class ArcCreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);
            var width = h.GetWidth();

            h.SetY(6);

            h.SetWidth(width / 4f);

            h.LabelField("待:");
            h.SetX(width / 16f);
            h.PropertyField("wait", false);

            h.SetX(width / 3.1f);
            h.LabelField("X:");
            h.SetX(width / 2.6f);
            h.PropertyField("x", false);

            h.SetX(width / 1.5f);
            h.PropertyField("vertexType", false);

            h.SetY();
            h.SetWidth(width / 2f);
            var disableProp = h.PropertyField("isJudgeDisable", overrideName: "Disable");

            using (new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                h.SetX(width / 2f);
                h.PropertyField("isOverlappable", overrideName: "Overlap");

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
            h.SetY(28);
            h.DrawLine(weight: 2);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 80;
        }
    }
}
