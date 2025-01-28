using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novel.Editor
{
    public static class FlowchartEditorUtility
    {
        /// <summary>
        /// ��΃p�X����"Assets/"�̃p�X�ɕϊ����܂�
        /// </summary>
        public static string AbsoluteToAssetsPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        /// <summary>
        /// C#�X�N���v�g������p�X���������܂�
        /// </summary>
        public static bool TryGetScriptPath(string fileName, out string relativePath)
        {
            string path = null;
            var pathes = AssetDatabase.FindAssets(fileName + " t:Script")
                .Select(AssetDatabase.GUIDToAssetPath);
            var novelPathes = pathes.Where(s => s.Contains("Novel"));
            if (novelPathes.Count() == 0)
            {
                path = pathes.FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                    fileName, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                path = novelPathes.FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                    fileName, StringComparison.CurrentCultureIgnoreCase));
            }

            bool isExist = !string.IsNullOrEmpty(path);
            if (isExist)
            {
                relativePath = AbsoluteToAssetsPath(path);
            }
            else
            {
                relativePath = string.Empty;
            }
            return isExist;
        }

        /// <summary>
        /// �w�肵�����O����ɁA�t�H���_���ŏd�����Ȃ����O�𐶐����܂�
        /// </summary>
        /// <param name="folderPath">�ΏۂƂȂ�t�H���_�̃p�X</param>
        /// <param name="baseName">���O(�g���q�͏���)</param>
        /// <param name="extension">�g���q(.�͏���)</param>
        /// <returns></returns>
        public static string GenerateAssetName(string folderPath, string baseName, string extension = null)
        {
            var paths = AssetDatabase.FindAssets(null, new string[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath);

            var existingNames = new HashSet<string>();
            foreach (var p in paths)
            {
                // �T�u�t�H���_�͌������Ȃ��Ă����̂ŏ��O
                if (AbsoluteToAssetsPath(Path.GetDirectoryName(p)) != folderPath.TrimEnd('/')) continue;
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(p);
                foreach (var o in objs)
                {
                    existingNames.Add(o.name);
                }
            }

            string trimedName = baseName;
            string regex = @" \( ?\d+\)$";
            if (Regex.IsMatch(baseName, regex))
            {
                trimedName = Regex.Replace(baseName, regex, string.Empty);
            }

            string exStr = string.IsNullOrEmpty(extension) ? string.Empty : $".{extension}";

            // �x�[�X�����d�����Ȃ��ꍇ�A���̂܂ܕԂ�
            if (!existingNames.Contains(trimedName))
            {
                return $"{trimedName}{exStr}";
            }

            // �d������ꍇ�A�u(n)�v��t���ă��j�[�N�Ȗ��O��T��
            int suffix = 1;
            while (true)
            {
                string newName = $"{trimedName} ({suffix})";
                if (!existingNames.Contains(newName))
                {
                    return $"{newName}{exStr}";
                }
                suffix++;
            }
        }

        public static T[] GetAllScriptableObjects<T>(string folderName = null) where T : ScriptableObject
        {
            string[] guids = folderName == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folderName });
            return guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
        }

        /// <summary>
        /// CommandData���쐬���AFlowchartData�̎q�ɐݒ肵�܂�
        /// </summary>
        /// <param name="parentData">�e�ƂȂ�FlowchartData</param>
        /// <param name="baseName">���O</param>
        /// <returns></returns>
        public static CommandData CreateSubCommandData(FlowchartData parentData, string baseName)
        {
            var createdCmd = ScriptableObject.CreateInstance<CommandData>();
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            createdCmd.name = GenerateAssetName(folderPath, baseName);
            AssetDatabase.AddObjectToAsset(createdCmd, parentData);
            Undo.RegisterCreatedObjectUndo(createdCmd, "Create Command");
            AssetDatabase.SaveAssets();
            return createdCmd;
        }

        public static CommandData DuplicateSubCommandData(FlowchartData parentData, CommandData cmdData)
        {
            var duplicatedCmd = Object.Instantiate(cmdData);
            string path = AssetDatabase.GetAssetPath(parentData);
            int lastIndex = path.LastIndexOf('/');
            string folderPath = path.Substring(0, lastIndex);
            duplicatedCmd.name = GenerateAssetName(folderPath, cmdData.name);
            AssetDatabase.AddObjectToAsset(duplicatedCmd, parentData);
            Undo.RegisterCreatedObjectUndo(duplicatedCmd, "Duplicate Command");
            AssetDatabase.SaveAssets();
            return duplicatedCmd;
        }

        /// <summary>
        /// ���g�p��CommandData���폜���܂�
        /// </summary>
        public static void DestroyAllUnusedCommandData()
        {
            int removeCount = 0;
            foreach (var data in GetAllScriptableObjects<FlowchartData>())
            {
                removeCount += DestroyUnusedCommandData(data);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            if (removeCount > 0)
            {
                Debug.Log($"���g�p�̃R�}���h�����v{removeCount}�폜����܂���");
            }
            else
            {
                Debug.Log($"�s�v�ȃf�[�^�̌������������܂����B�폜���ꂽ�f�[�^�͂���܂���");
            }


            // �w�肵��FlowchartData�Ŏg���Ă��Ȃ��R�}���h���폜
            static int DestroyUnusedCommandData(FlowchartData flowchartData)
            {
                int removeCount = 0;
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(flowchartData))
                    .Where(x => AssetDatabase.IsSubAsset(x));
                foreach (var sub in subAssets)
                {
                    CommandData cmdData = sub as CommandData;
                    if (flowchartData.IsUsed(cmdData) == false)
                    {
                        removeCount++;
                        Object.DestroyImmediate(cmdData, true);
                    }
                }

                if (removeCount > 0)
                {
                    Debug.Log($"{flowchartData.name} �Ŗ��g�p�̃R�}���h��{removeCount}�폜����܂���");
                }
                return removeCount;
            }
        }
    }
}