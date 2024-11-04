using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.ShortcutManagement;

namespace NoteGenerating.Editor
{
    public static class NotePreviewShortCut
    {
        [Shortcut("Preview Notes", KeyCode.Period, ShortcutModifiers.None)]
        public static void PreviewNotes()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != ConstContainer.InGameSceneName
                && sceneName != ConstContainer.TestSceneName) return;
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
}