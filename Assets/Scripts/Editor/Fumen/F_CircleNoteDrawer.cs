using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(F_CircleNote.NoteData))]
    public class F_CircleNoteDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);
            float width = h.GetWidth();
            float labelWidth = width * 0.2f;
            float boxWidth = width * 0.12f;

            h.SetXAsWidth(0.8f);
            var disabledProp = h.PropertyField(boxWidth, "disabled", false);

            using (new EditorGUI.DisabledGroupScope(disabledProp.boolValue))
            {
                h.SetXAsWidth(0f);
                h.LabelField(labelWidth, "Pos");

                h.SetXAsWidth(0.1f);
                h.PropertyField(boxWidth, "x", false);
                h.SetXAsWidth(0.25f);
                h.PropertyField(boxWidth, "y", false);
            }

            h.SetXAsWidth(0.45f);
            h.LabelField(labelWidth, "Wait");

            h.SetXAsWidth(0.55f);
            var waitProp = h.PropertyField(boxWidth, "wait", false);

            if (waitProp.floatValue == 0f)
            {
                h.DrawBox(new Rect(20, position.y, width + 40, 2f * 22), Color.yellow);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => DrawerHelper.Height;
    }
}