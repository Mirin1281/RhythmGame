using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(NoteData))]
    public class F_Generic2D_NoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);
            var width = h.GetWidth();

            h.LabelField("待:");
            h.SetX(h.GetX() - 26f);
            var waitProp = h.PropertyField(width * 0.14f, "wait", false);

            h.SetX(width / 4f * 1f);
            var typeProp = h.PropertyField(width * 0.2f, "type", false);
            var type = (RegularNoteType)typeProp.enumValueIndex;

            if (type == RegularNoteType._None)
            {
                h.DrawBox(new Rect(19, position.y - 2, width + 40, 22), Color.cyan);
                return;
            }

            h.SetX(width / 4f * 2f);
            h.LabelField("X:");
            h.SetX(h.GetX() - 30f);
            h.PropertyField(width * 0.14f, "x", false);

            if (type == RegularNoteType.Hold)
            {
                h.SetX(width / 4f * 3f);
                h.LabelField("長:");
                h.SetX(h.GetX() - 30f);
                h.PropertyField(width * 0.14f, "length", false);
            }

            if (waitProp.floatValue == 0f && h.GetArrayElementIndex() != 0)
            {
                h.DrawBox(new Rect(19, position.y - 22, width + 40, 2f * 20), Color.yellow);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => DrawerHelper.Height;
    }
}