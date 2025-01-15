using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NoteGenerating.Editor
{
    // GenerateDataは普通に削除しても空のassetファイルが残ります
    // これはUndo操作に対応するためですが、再起動などでも消えないため明示的に削除する必要があります
    //
    // このクラスではエディタの終了時とビルドの直前にそれらのファイルの掃除をします
    // またFumenDataの"未使用のGenerateDataを削除"でも掃除ができます
    public class RemainGenerateDataRemover : IPreprocessBuildWithReport
    {
        [InitializeOnLoadMethod]
        static void StartRemoveTrash()
        {
            EditorApplication.quitting -= RemoveUnusedGenerateData;
            EditorApplication.quitting += RemoveUnusedGenerateData;
        }

        static void RemoveUnusedGenerateData()
        {
            FumenEditorUtility.DestroyAllUnusedGenerateData();
        }

        public int callbackOrder => 0;

        // Console > Clear > "Clear on Recompile" 
        // のチェックを外さないと、この中で呼び出されたログが表示されません
        public void OnPreprocessBuild(BuildReport report)
        {
            RemoveUnusedGenerateData();
        }
    }
}