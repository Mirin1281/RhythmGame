using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.ShortcutManagement;

namespace NoteCreating.Editor
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
            if (window == null) return;
            var command = window.GetLastSelectedCommand();
            if (command == null) return;
            var commandBase = command.GetCommandBase();
            if (commandBase == null) return;

            Undo.RecordObject(command, "Preview Command");
            commandBase.Preview();
        }
    }
}