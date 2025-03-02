using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(CameraMoveSetting))]
    public class CameraMoveSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);
            var startW = h.GetWidth();
            h.SetY(10);

            h.PropertyField("wait");
            h.SetY();
            h.SetY(10);

            h.LabelField("Pos");
            h.SetX(60);

            var isPosMove_p = h.PropertyField("isPosMove", false);
            h.SetX(EditorGUIUtility.labelWidth);

            using (new EditorGUI.DisabledGroupScope(isPosMove_p.boolValue == false))
            {
                h.SetX(EditorGUIUtility.labelWidth);
                h.PropertyField("pos", overrideName: string.Empty);
                h.SetY();
            }

            h.LabelField("Rotate");
            h.SetX(60);

            var isRotateMove_p = h.PropertyField("isRotateMove", false);
            h.SetX(EditorGUIUtility.labelWidth);

            using (new EditorGUI.DisabledGroupScope(isRotateMove_p.boolValue == false))
            {
                h.SetX(EditorGUIUtility.labelWidth);
                h.PropertyField("rotate", overrideName: string.Empty);
                h.SetY();
                h.PropertyField("isRotateClamp", overrideName: "Rotate Clamp");
                h.SetY();
            }

            h.LabelField("Easing");
            h.SetX(EditorGUIUtility.labelWidth);

            float w = (h.GetWidth() - EditorGUIUtility.labelWidth) / 2f;
            h.SetWidth(w);
            h.PropertyField("easeType", false);
            h.SetX(EditorGUIUtility.labelWidth + w);

            h.PropertyField("lpb", false);
            h.SetY();

            h.PropertyField("moveType");
            h.SetY();
            h.SetY(10);

            h.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 160;
        }
    }
}
