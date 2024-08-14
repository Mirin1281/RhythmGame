using System.Reflection;
using UnityEngine;

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var button = attribute as ButtonAttribute;

            if (GUI.Button(position, string.IsNullOrEmpty(button.label) ? label.text : button.label))
            {
                var obj = property.serializedObject.targetObject;
                MethodInfo method = obj.GetType().GetMethod(button.method, BindingFlags.Public | BindingFlags.Instance);
                Debug.Log(method);
                method.Invoke(obj, null);
            }
        }
    }
}