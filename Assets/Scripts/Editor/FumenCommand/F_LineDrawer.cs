using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(F_Line.LineCreateData))]
    public class F_LineDrawer : PropertyDrawer
    {
        const float Height = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hel = new PropertyDrawerHelper(position, property, Height);
            hel.SetY();

            hel.PropertyField("delayLPB");
            hel.SetY(Height / 2f);

            var isPosEaseProp = hel.PropertyField("isPosEase");
            hel.SetY();
            hel.IndentLevel++;
            if(isPosEaseProp.boolValue)
            {
                hel.LabelField("Start");
                hel.SetX(EditorGUIUtility.labelWidth);
                hel.PropertyField(hel.Width / 1.8f, "startPos", false);
                hel.SetY();

                hel.LabelField("From");
                hel.SetX(EditorGUIUtility.labelWidth);
                hel.PropertyField(hel.Width / 1.8f, "fromPos", false);
                hel.SetY();

                hel.PropertyField("overridePosEaseTime", overrideName: "OverrideTime");
                hel.SetY();
                hel.PropertyField("overridePosEaseType", overrideName: "OverrideType");
                hel.SetY();
            }
            else
            {
                hel.LabelField("Pos");
                hel.SetX(EditorGUIUtility.labelWidth);
                hel.PropertyField(hel.Width / 1.8f, "startPos", false);
                hel.SetY();
            }
            hel.IndentLevel--;


            var isRotateEaseProp = hel.PropertyField("isRotateEase");
            hel.SetY();
            hel.IndentLevel++;
            if(isRotateEaseProp.boolValue)
            {
                hel.PropertyField("startRotate", overrideName: "Start");
                hel.SetY();

                hel.PropertyField("fromRotate", overrideName: "From");
                hel.SetY();

                hel.PropertyField("overrideRotateEaseTime", overrideName: "OverrideTime");
                hel.SetY();
                hel.PropertyField("overrideRotateEaseType", overrideName: "OverrideType");
                hel.SetY();
            }
            else
            {
                hel.PropertyField("startRotate", overrideName: "Rotate");
                hel.SetY();
            }
            hel.IndentLevel--;


            var isAlphaEaseProp = hel.PropertyField("isAlphaEase");
            hel.SetY();
            hel.IndentLevel++;
            if(isAlphaEaseProp.boolValue)
            {
                hel.PropertyField("startAlpha", overrideName: "Start");
                hel.SetY();

                hel.PropertyField("fromAlpha", overrideName: "From");
                hel.SetY();

                hel.PropertyField("overrideAlphaEaseTime", overrideName: "OverrideTime");
                hel.SetY();
                hel.PropertyField("overrideAlphaEaseType", overrideName: "OverrideType");
                hel.SetY();
            }
            else
            {
                hel.PropertyField("startAlpha", overrideName: "Alpha");
                hel.SetY();
            }
            hel.IndentLevel--;


            var isRotateFromPosProp = hel.PropertyField("isRotateFromPos");
            hel.SetY();
            if(isRotateFromPosProp.boolValue)
            {
                hel.IndentLevel++;
                hel.PropertyField("rotateFromPos");
                hel.SetY();
                hel.PropertyField("centerPos");
                hel.SetY();
                hel.SetY();
                hel.IndentLevel--;
            }
            hel.SetY(-Height / 2f);

            hel.DrawLine();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int count = 11;

            if(property.FindPropertyRelative("isPosEase").boolValue)
            {
                count += 3;
            }
            if(property.FindPropertyRelative("isRotateEase").boolValue)
            {
                count += 3;
            }
            if(property.FindPropertyRelative("isAlphaEase").boolValue)
            {
                count += 3;
            }
            if(property.FindPropertyRelative("isRotateFromPos").boolValue)
            {
                count += 3;
            }

            return Height * count;
        }
    }
}