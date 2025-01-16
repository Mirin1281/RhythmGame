using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(F_Line.LineCreateData))]
    public class F_LineDrawer : PropertyDrawer
    {
        const float Height = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property, Height);
            h.SetY();

            h.PropertyField("delayLPB");
            h.SetY(Height / 2f);

            var isPosEaseProp = h.PropertyField("isPosEase");
            h.SetY();
            h.SetIndentLevel(true);
            if (isPosEaseProp.boolValue)
            {
                h.LabelField("Start");
                h.SetX(EditorGUIUtility.labelWidth);
                h.PropertyField(h.GetWidth() / 1.8f, "startPos", false);
                h.SetY();

                h.LabelField("From");
                h.SetX(EditorGUIUtility.labelWidth);
                h.PropertyField(h.GetWidth() / 1.8f, "fromPos", false);
                h.SetY();

                h.PropertyField("overridePosEaseTime", overrideName: "OverrideTime");
                h.SetY();
                h.PropertyField("overridePosEaseType", overrideName: "OverrideType");
                h.SetY();
            }
            else
            {
                h.LabelField("Pos");
                h.SetX(EditorGUIUtility.labelWidth);
                h.PropertyField(h.GetWidth() / 1.8f, "startPos", false);
                h.SetY();
            }
            h.SetIndentLevel(false);


            var isRotateEaseProp = h.PropertyField("isRotateEase");
            h.SetY();
            h.SetIndentLevel(true);
            if (isRotateEaseProp.boolValue)
            {
                h.PropertyField("startRotate", overrideName: "Start");
                h.SetY();

                h.PropertyField("fromRotate", overrideName: "From");
                h.SetY();

                h.PropertyField("overrideRotateEaseTime", overrideName: "OverrideTime");
                h.SetY();
                h.PropertyField("overrideRotateEaseType", overrideName: "OverrideType");
                h.SetY();
            }
            else
            {
                h.PropertyField("startRotate", overrideName: "Rotate");
                h.SetY();
            }
            h.SetIndentLevel(false);


            var isAlphaEaseProp = h.PropertyField("isAlphaEase");
            h.SetY();
            h.SetIndentLevel(true);
            if (isAlphaEaseProp.boolValue)
            {
                h.PropertyField("startAlpha", overrideName: "Start");
                h.SetY();

                h.PropertyField("fromAlpha", overrideName: "From");
                h.SetY();

                h.PropertyField("overrideAlphaEaseTime", overrideName: "OverrideTime");
                h.SetY();
                h.PropertyField("overrideAlphaEaseType", overrideName: "OverrideType");
                h.SetY();
            }
            else
            {
                h.PropertyField("startAlpha", overrideName: "Alpha");
                h.SetY();
            }
            h.SetIndentLevel(false);


            var isRotateFromPosProp = h.PropertyField("isRotateFromPos");
            h.SetY();
            if (isRotateFromPosProp.boolValue)
            {
                h.SetIndentLevel(true);
                h.PropertyField("rotateFromPos");
                h.SetY();
                h.PropertyField("centerPos");
                h.SetY();
                h.SetY();
                h.SetIndentLevel(false);
            }
            h.SetY(-Height / 2f);

            h.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int count = 11;

            if (property.FindPropertyRelative("isPosEase").boolValue)
            {
                count += 3;
            }
            if (property.FindPropertyRelative("isRotateEase").boolValue)
            {
                count += 3;
            }
            if (property.FindPropertyRelative("isAlphaEase").boolValue)
            {
                count += 3;
            }
            if (property.FindPropertyRelative("isRotateFromPos").boolValue)
            {
                count += 3;
            }

            return Height * count;
        }
    }
}