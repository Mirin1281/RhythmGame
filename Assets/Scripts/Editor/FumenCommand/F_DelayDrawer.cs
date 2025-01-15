using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Delay))]
    public class F_DelayDrawer : PropertyDrawer
    {
        const float Height = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property, Height);

            h.PropertyField("wait");
            h.SetY();
            h.SetIndentLevel(false);
            h.PropertyField("noteGeneratable");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => Height;
    }
}