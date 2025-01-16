using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NoteCreating.Editor
{
    public class RemainCommandDataRemover : IPreprocessBuildWithReport
    {
        [InitializeOnLoadMethod]
        static void StartRemoveTrash()
        {
            EditorApplication.quitting -= RemoveUnusedCommandData;
            EditorApplication.quitting += RemoveUnusedCommandData;
        }

        static void RemoveUnusedCommandData()
        {
            FumenEditorUtility.DestroyAllUnusedCommandData();
        }

        public int callbackOrder => 0;

        // Console > Clear > "Clear on Recompile" 
        // のチェックを外さないと、この中で呼び出されたログが表示されません
        public void OnPreprocessBuild(BuildReport report)
        {
            RemoveUnusedCommandData();
        }
    }
}