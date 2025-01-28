using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    /// <summary>
    /// CharacterData�ɂ���ƃh���b�v�_�E�������őI���ł��܂�
    /// </summary>
    [CustomPropertyDrawer(typeof(DropDownCharacterAttribute))]
    public class DropDownCharacterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            CommandDrawerUtility.DropDownCharacterList(rect, property);
        }
    }
}
