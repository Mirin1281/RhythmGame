using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(EasingStatus_Lpb))]
    public class EasingStatusDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);
            h.SetWidth(h.GetWidth() / 4);
            var w = h.GetWidth();

            h.PropertyField(w - 4, "start", false);

            h.SetX(w - 4);
            h.PropertyField(w - 4, "from", false);

            h.SetX(w * 2 - 8);
            h.PropertyField(w + 8, "easeType", false);

            h.SetX(w * 3);
            h.PropertyField("easeLpb", false);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawerHelper.Height;
        }
    }
}
