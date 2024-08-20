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

            if (GUILayout.Button("�t���[�`���[�g�G�f�B�^���J��"))
            {
                EditorWindow.GetWindow<FumenEditorWindow>("Fumen Editor", typeof(SceneView));
            }

            EditorGUILayout.Space(20);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("��������"))
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
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("�폜����"))
                {
                    var FumenData = target as FumenData;
                    foreach (var cmdData in FumenData.Fumen.GetGenerateDataList())
                    {
                        FumenEditorUtility.DestroyScritableObject(cmdData);
                    }
                    FumenEditorUtility.DestroyScritableObject(FumenData);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                if (GUILayout.Button("���g�p��NoteGeneratorData���폜����"))
                {
                    var cmdDatas = FumenEditorUtility.GetAllScriptableObjects<GenerateData>();
                    var FumenDatas = FumenEditorUtility.GetAllScriptableObjects<FumenData>();

                    foreach (var cmdData in cmdDatas)
                    {
                        if (IsUsed(cmdData, FumenDatas) == false)
                        {
                            FumenEditorUtility.DestroyScritableObject(cmdData);
                        }
                    }
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
            }

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CSV�`���ŃG�N�X�|�[�g����"))
                {
                    FumenCSVIO.ExportFumenDataAsync(target as FumenData).Forget();
                }
                if (GUILayout.Button("CSV���C���|�[�g����"))
                {
                    FumenCSVIO.ImportFumenDataAsync(target as FumenData).Forget();
                }
            }
        }        

        bool IsUsed(GenerateData targetData, FumenData[] FumenDatas)
        {
            foreach (var FumenData in FumenDatas)
            {
                if (FumenData.Fumen.IsUsed(targetData)) return true;
            }
            return false;
        }
    }
}