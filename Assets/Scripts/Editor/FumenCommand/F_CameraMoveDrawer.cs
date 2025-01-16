using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(CameraMoveSetting))]
    public class F_CameraMoveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property);
            var width = h.GetWidth();

            h.PropertyField("wait");

            h.SetY(10);
            h.LabelField("Pos");
            h.SetX(60);
            var isPosMove_p = h.PropertyField("isPosMove", false);
            h.SetX(EditorGUIUtility.labelWidth);
            h.SetWidth(width - EditorGUIUtility.labelWidth);
            using (new EditorGUI.DisabledGroupScope(isPosMove_p.boolValue == false))
            {
                h.PropertyField("pos", false);
            }

            h.SetY();
            h.LabelField("Rotate");
            h.SetX(60);
            var isRotateMove_p = h.PropertyField("isRotateMove", false);
            h.SetX(EditorGUIUtility.labelWidth);
            h.SetWidth(width - EditorGUIUtility.labelWidth);
            using (new EditorGUI.DisabledGroupScope(isRotateMove_p.boolValue == false))
            {
                h.PropertyField("rotate", false);
                h.SetY();
                h.PropertyField("isRotateClamp");
            }

            h.SetY();
            h.PropertyField("time");
            h.SetY();
            h.PropertyField("easeType");
            h.SetY();
            h.PropertyField("moveType");
            h.SetY();
            h.SetY(-10);
            h.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20 * 8.5f;
        }
    }
}
