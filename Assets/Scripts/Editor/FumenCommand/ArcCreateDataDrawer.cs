using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(ArcCreateData))]
    public class ArcCreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property);
            var width = hel.StartWidth;

            hel.PropertyField(width / 2f, "pos", false);

            hel.SetX(width / 1.8f);
            hel.PropertyField(width / 2.2f, "vertexMode", false);

            hel.SetY();
            hel.SetWidth(width / 2f);
            var disableProp = hel.PropertyField("isJudgeDisable");

            using (new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                hel.SetX(width / 2f);
                hel.PropertyField("isDuplicated");

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
            hel.SetY(10);
            hel.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 4f * 20;
        }
    }
}
