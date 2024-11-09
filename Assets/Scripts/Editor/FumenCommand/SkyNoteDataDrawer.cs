using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Sky.SkyNoteData))]
    public class SkyNoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property);
            var width = h.GetWidth();

            h.SetXAsWidth(0.04f);
            var disableProp = h.PropertyField("disable", false);

            h.SetXAsWidth(0.12f);
            h.SetWidth(width / 8f);
            h.LabelField("待:");
            h.SetX(h.GetX() - 25);
            var waitProp = h.PropertyField("wait", false);

            using(new EditorGUI.DisabledGroupScope(disableProp.boolValue))
            {
                h.SetXAsWidth(0.35f);
                h.SetWidth(width / 3f);
                h.PropertyField("pos", false);
            }            

            if(waitProp.floatValue != 0f) return;
            if(disableProp.boolValue)
            {
                h.DrawBox(new Rect(19, position.y -2, width + 40, EditorGUIUtility.singleLineHeight + 4), Color.cyan);
            }
            else
            {
                h.DrawBox(new Rect(19, position.y -20, width + 40, 2 * (EditorGUIUtility.singleLineHeight + 4)), Color.yellow);
            }
        }
    }
}