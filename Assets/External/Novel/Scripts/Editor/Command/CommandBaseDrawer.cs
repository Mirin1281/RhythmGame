using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomPropertyDrawer(typeof(CommandBase), useForChildren: false)]
    public class CommandBaseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { }

        /// <summary>
        /// �t�B�[���h��`�悵�܂�
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="fieldName">�t�B�[���h�̖��O</param>
        /// <returns>�t�B�[���h�̃v���p�e�B</returns>
        protected SerializedProperty DrawField(ref Rect position, SerializedProperty property, string fieldName)
        {
            var prop = property.FindPropertyRelative(fieldName);
            EditorGUI.PropertyField(position, prop);
            if(prop.isArray)
            {
                position.y += GetArrayHeight(prop);
            }
            else
            {
                position.y += GetHeight();
            }
            return prop;
        }

        protected float GetHeight(float? height = null) => height ?? EditorGUIUtility.singleLineHeight;

        static float GetArrayHeight(SerializedProperty property)
        {
            if (property.isArray == false)
            {
                Debug.LogWarning("�v���p�e�B���z��ł͂���܂���I");
                return EditorGUIUtility.singleLineHeight;
            }
            if (property.isExpanded == false)
            {
                return EditorGUIUtility.singleLineHeight * 1.5f;
            }
            int length = property.arraySize;
            if (length is 0 or 1)
            {
                return EditorGUIUtility.singleLineHeight * 4f;
            }
            else
            {
                return (length + 3) * EditorGUIUtility.singleLineHeight;
            }
        }
    }
}