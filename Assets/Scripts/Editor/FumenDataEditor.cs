using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

namespace NoteGenerating.Editor
{
    [CustomEditor(typeof(FumenData))]
    public class FumenDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);

            if (GUILayout.Button("フローチャートエディタを開く"))
            {
                EditorWindow.GetWindow<FumenEditorWindow>("Fumen Editor", typeof(SceneView));
            }

            EditorGUILayout.Space(20);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("複製する"))
                {
                    var fumenData = target as FumenData;
                    var copiedFumenData = Instantiate(fumenData);
                    copiedFumenData.name = target.name;
                    var folderPath = FumenEditorUtility.GetExistFolderPath(fumenData);

                    var dataName = FumenEditorUtility.GetFileName(folderPath, copiedFumenData.name, "asset");
                    AssetDatabase.CreateAsset(copiedFumenData, Path.Combine(folderPath, dataName));
                    AssetDatabase.ImportAsset(folderPath, ImportAssetOptions.ForceUpdate);
                    var Fumen = copiedFumenData.Fumen;

                    var copiedCmdList = new List<GenerateData>();
                    foreach (var cmdData in Fumen.GetReadOnlyGenerateDataList())
                    {
                        if(cmdData == null) continue;
                        var copiedCmdData = Instantiate(cmdData);
                        var path = FumenEditorUtility.GetExistFolderPath(cmdData);
                        var cmdName = FumenEditorUtility.GetFileName(path, $"NoteGeneratorData_{copiedFumenData.name}", "asset");
                        AssetDatabase.CreateAsset(copiedCmdData, Path.Combine(path, cmdName));
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        copiedCmdList.Add(copiedCmdData);
                    }
                    Fumen.SetGenerateDataList(copiedCmdList);
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(copiedFumenData);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("削除する"))
                {
                    var FumenData = target as FumenData;
                    foreach (var cmdData in FumenData.Fumen.GetGenerateDataList())
                    {
                        FumenEditorUtility.DestroyScritableObject(cmdData);
                    }
                    FumenEditorUtility.DestroyScritableObject(FumenData);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("未使用のGeneratorDataを削除"))
                {
                    FumenEditorUtility.RemoveUnusedGenerateData();
                }
            }

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV形式でエクスポートする"))
                {
                    FumenCSVIO.ExportFumenDataAsync(target as FumenData).Forget();
                }
                if (GUILayout.Button("CSVをインポートする"))
                {
                    FumenCSVIO.ImportFumenDataAsync(target as FumenData).Forget();
                }
            }
        }
    }
}