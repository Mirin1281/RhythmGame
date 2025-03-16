using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(CommandSelectAttribute))]
    public class CommandSelectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selfCommand = property.serializedObject.targetObject as CommandData;
            PopUpCommand(selfCommand, position, property);
        }

        CommandData PopUpCommand(CommandData selfCommand, Rect position, SerializedProperty property)
        {
            // フィールドの譜面内のコマンドを取得
            var commands = selfCommand.GetFumenData().Fumen.GetCommandDataList().Prepend(null).ToArray();
            int previousIndex = Array.IndexOf(commands, property.objectReferenceValue as CommandData);
            int selectedIndex = EditorGUI.Popup(
                position, property.displayName, previousIndex, GetPopUpNames(commands, selfCommand));

            if (previousIndex != selectedIndex)
            {
                property.objectReferenceValue = commands[selectedIndex];
                property.serializedObject.ApplyModifiedProperties();
            }

            if (selectedIndex < commands.Length && selectedIndex >= 0)
            {
                return commands[selectedIndex];
            }
            else
            {
                return null;
            }
        }

        string[] GetPopUpNames(CommandData[] commands, CommandData selfCommand)
        {
            int selfIndex = Array.IndexOf(commands, selfCommand);
            int index = 0;
            return commands.Select(c =>
            {
                string display = string.Empty;
                if (c == null)
                {
                    index++;
                    return "Null";
                }

                if (selfIndex - 10 > index)
                {
                    display = "Other↑/";
                }
                else if (selfIndex + 10 < index)
                {
                    display = "Other↓/";
                }

                display += $"{c.BeatTiming}:  {c.GetName()} ({index})";
                if (c == selfCommand)
                {
                    display = "◆ " + display;
                }
                index++;
                return display;
            }).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return DrawerHelper.Height;
        }
    }
}