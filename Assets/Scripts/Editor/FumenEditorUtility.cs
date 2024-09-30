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
            string path = ConstContainer.DATA_PATH;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var name = GetFileName(path, baseName, "asset");
            var data = ScriptableObject.CreateInstance<GenerateData>();
            AssetDatabase.CreateAsset(data, Path.Combine(path, name));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return data;
        }

        /// <summary>
        /// �s�v��GenerateData���폜���܂�
        /// </summary>
        public static void RemoveUnusedGenerateData(string folderPath = null)
        {
            // Undo���Ŕj����Ԃ̃t�@�C�����폜 //
            var guids = AssetDatabase.FindAssets(null, new string[] { folderPath ??= ConstContainer.DATA_PATH });
            int removeCount = 0;
            for(int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var obj = AssetDatabase.LoadAllAssetsAtPath(path);
                if(obj.Length == 0) // �A�Z�b�g���Ȃ� > �j�����Ă��� 
                {
                    File.Delete(path);
                    File.Delete($"{path}.meta");
                    removeCount++;
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // �ǂ̃t���[�`���[�g�ɂ������Ă��Ȃ�CommandData���폜 //
            var datas = GetAllScriptableObjects<GenerateData>();
            var fumenDatas = GetAllScriptableObjects<FumenData>();
            foreach (var d in datas)
            {
                if (IsUsed(d, fumenDatas) == false)
                {
                    removeCount++;
                    DestroyScritableObject(d);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            if(removeCount > 0)
            {
                Debug.Log($"�s�v�ȃf�[�^��{removeCount}�폜����܂���\nFolder: {folderPath}");
            }
            else
            {
                Debug.Log($"�s�v�ȃf�[�^�̌������������܂����B�폜���ꂽ�f�[�^�͂���܂���\nFolder: {folderPath}");
            }
            

            static bool IsUsed(GenerateData targetData, FumenData[] fumenDatas)
            {
                foreach (var flowchartData in fumenDatas)
                {
                    if (flowchartData.Fumen.IsUsed(targetData)) return true;
                }
                return false;
            }
        }
    }
}