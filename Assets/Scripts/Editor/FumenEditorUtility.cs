using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NoteGenerating.Editor
{
    public static class FumenEditorUtility
    {
        /// <summary>
        /// ��΃p�X���� Assets/�̃p�X�ɕϊ�����
        /// </summary>
        public static string AbsoluteToAssetsPath(string path)
        {
            return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        public static void DestroyScritableObject(ScriptableObject obj)
        {
            var path = GetExistFolderPath(obj);
            var deleteName = obj.name;
            Object.DestroyImmediate(obj, true);
            File.Delete($"{path}/{deleteName}.asset");
            File.Delete($"{path}/{deleteName}.asset.meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        // �v���W�F�N�g��ɂ���A�Z�b�g�̃p�X��Ԃ��܂�
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// �t�@�C�������d�����l�����ĕt���܂�
        /// </summary>
        /// <param name="path">�p�X</param>
        /// <param name="name">���O(�g���q�͏���)</param>
        /// <param name="extension">�g���q(.�͏���)</param>
        /// <returns></returns>
        public static string GetFileName(string path, string name, string extension)
        {
            int i = 1;
            var targetName = name;
            while (File.Exists($"{path}/{targetName}.{extension}"))
            {
                targetName = $"{name} {i++}";
            }
            return $"{targetName}.{extension}";
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
        /// GenerateData���쐬���܂�
        /// </summary>
        /// <param name="baseName">���O</param>
        /// <returns></returns>
        public static GenerateData CreateGenerateData(string baseName)
        {
            string path =ConstContainer.DATA_PATH;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var name = GetFileName(path, baseName, "asset");
            var cmdData = ScriptableObject.CreateInstance<GenerateData>();
            AssetDatabase.CreateAsset(cmdData, Path.Combine(path, name));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return cmdData;
        }
    }
}