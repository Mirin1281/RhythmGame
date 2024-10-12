using UnityEngine;
using UnityEditor;
using System.Reflection;
using NoteGenerating.Editor;
using UnityEngine.SceneManagement;

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
                    if (Event.current.keyCode == KeyCode.Period || Event.current.keyCode == KeyCode.Slash)
                    {
                        if (SceneManager.GetActiveScene().name != ConstContainer.InGameSceneName
                         && SceneManager.GetActiveScene().name != ConstContainer.TestSceneName) return;
                        var window = EditorWindow.GetWindow<FumenEditorWindow>();
                        if(window == null) return;
                        var command = window.LastSelectedCommand;
                        if(command == null) return;
                        var generatorBase = command.GetNoteGeneratorBase();
                        if(generatorBase == null) return;
                        
                        generatorBase.Preview();
                        Undo.RecordObject(command, "NotePreview");
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