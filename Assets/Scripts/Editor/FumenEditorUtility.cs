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
        /// 絶対パスから Assets/のパスに変換する
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

        // プロジェクト上にあるアセットのパスを返します
        public static string GetExistFolderPath(Object obj)
        {
            var dataPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (string.IsNullOrEmpty(dataPath)) return null;
            var index = dataPath.LastIndexOf("/");
            return dataPath.Substring(0, index);
        }

        /// <summary>
        /// ファイル名を重複を考慮して付けます
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="name">名前(拡張子は除く)</param>
        /// <param name="extension">拡張子(.は除く)</param>
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
        /// GenerateDataを作成します
        /// </summary>
        /// <param name="baseName">名前</param>
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
        /// 不要なGenerateDataを削除します
        /// </summary>
        public static void RemoveUnusedGenerateData(string folderPath = null)
        {
            // Undo等で破損状態のファイルを削除
            var guids = AssetDatabase.FindAssets(null, new string[] { folderPath ??= ConstContainer.DATA_PATH });
            int removeCount = 0;
            for(int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var obj = AssetDatabase.LoadAllAssetsAtPath(path);
                if(obj.Length == 0) // アセットがない = 破損している 
                {
                    File.Delete(path);
                    File.Delete($"{path}.meta");
                    removeCount++;
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // どのフローチャートにも属していないCommandDataを削除
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
                Debug.Log($"不要なデータが{removeCount}個削除されました\nFolder: {folderPath}");
            }
            else
            {
                Debug.Log($"不要なデータの検索が完了しました。削除されたデータはありません\nFolder: {folderPath}");
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