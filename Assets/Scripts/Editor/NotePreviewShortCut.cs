using UnityEngine;
using UnityEditor;
using System.Reflection;
using NoteGenerating.Editor;

namespace NoteGenerating
{
    [InitializeOnLoad]
    public class NotePreviewShortCut
    {
        static NotePreviewShortCut()
        {
            bool keyDown = false;
            EditorApplication.CallbackFunction function = () =>
            {
                if (!keyDown && Event.current.type == EventType.KeyDown)
                {
                    keyDown = true;

                    if (Event.current.keyCode == KeyCode.Period && FumenEditorWindow.LastSelectedCommand != null)
                    {
                        FumenEditorWindow.LastSelectedCommand.Preview();
                        Undo.RecordObject(FumenEditorWindow.LastSelectedCommand, "NotePreview");
                    }
                }

                if (keyDown && Event.current.type == EventType.KeyUp)
                {
                    keyDown = false;
                }
            };

            FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
            functions += function;
            info.SetValue(null, functions);
        }
    }
}