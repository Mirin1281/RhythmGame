using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace NoteCreating.Editor
{
    public static class FumenAllExport
    {
        [MenuItem("Tools/Rhythm Game/Export All Fumen")]
        static void ExportAllFumen()
        {
            var fumenDatas = FumenEditorUtility.GetAllScriptableObjects<FumenData>();
            for (int i = 0; i < fumenDatas.Length; i++)
            {
                FumenCSVIO.ExportFumenDataAsync(fumenDatas[i]).Forget();
            }
        }
    }
}