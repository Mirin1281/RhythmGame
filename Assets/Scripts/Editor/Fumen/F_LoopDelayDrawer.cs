using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(F_LoopDelay))]
    public class F_LoopDelayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);

            h.PropertyField("delay");
            h.SetY();
            h.PropertyField("loopCount");
            h.SetY();
            h.PropertyField("loopWait");
            h.SetY();
            h.SetY(10);

            h.DrawLine();
            h.SetY(10);

            h.SetIndentLevel(false);
            h.PropertyField("command");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawerHelper.Height * 4f + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("command"));
        }
    }
}