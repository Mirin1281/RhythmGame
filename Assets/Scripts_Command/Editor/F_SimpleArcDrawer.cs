using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(F_SimpleArc.SimpleArcCreateData))]
    public class F_SimpleArcDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);

            float tmpWidth = h.GetWidth() / 14f;
            h.DrawBox(new Rect(position.x - tmpWidth, position.y, position.width + tmpWidth, DrawerHelper.Height), Color.cyan, 0.3f);
            h.PropertyField("wait");
            h.SetY(DrawerHelper.Height * 1.5f);

            h.PropertyField("point");
            h.SetY();

            h.PropertyField("length");
            h.SetY();

            h.SetWidth(h.GetWidth() / 2f);
            var isDetailedProp = h.PropertyField("isDetailed");

            h.SetX(h.GetWidth() * 0.95f);
            h.LabelField("Overlap");
            h.SetX(h.GetWidth() * 1.25f);
            h.PropertyField("isOverlappable", false);

            h.SetX(h.GetWidth() * 1.5f);
            h.LabelField("2Judge");
            h.SetX(h.GetWidth() * 1.8f);
            h.PropertyField("twoJudge", false);

            if (isDetailedProp.boolValue)
            {
                h.SetY();
                h.PropertyField("dir");
                h.SetY();
                h.PropertyField("curve");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isDetailedProp = property.FindPropertyRelative("isDetailed");
            return isDetailedProp.boolValue ? DrawerHelper.Height * 6.8f : DrawerHelper.Height * 4.8f;
        }
    }
}
