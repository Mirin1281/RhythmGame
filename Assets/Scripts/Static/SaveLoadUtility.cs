using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

public class SaveLoadUtility
{
#if UNITY_EDITOR
    readonly static int encryptKey = 0;
#else
    readonly static int encryptKey = 90;
#endif

    static string GetFilePath(string fileName)
        => $"{Application.persistentDataPath}/{fileName}.json";

    public static async UniTask SetData<T>(T saveData, string fileName) where T : JsonObject
    {
        var writer = new StreamWriter(GetFilePath(fileName), false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
        await writer.WriteAsync(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static void SetDataImmediately<T>(T saveData, string fileName) where T : JsonObject
    {
        var writer = new StreamWriter(GetFilePath(fileName), false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
        writer.Write(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static async UniTask<T> GetData<T>(string fileName, CancellationToken token = default) where T : JsonObject
    {
        string path = GetFilePath(fileName); 
        if(File.Exists(path) == false)
        {
            Debug.Log($"{path}が存在しませんでした");
            return null;
        }
        var reader = new StreamReader(path);
        var readString = reader.ReadToEnd();
        reader.Close();

        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);

        var loadedData = await UniTask.RunOnThreadPool(() =>
            JsonConvert.DeserializeObject<T>(decryptedString), cancellationToken: token
        );
        return loadedData;
    }

    public static T GetDataImmediately<T>(string fileName) where T : JsonObject
    {
        string path = GetFilePath(fileName);
        if(File.Exists(path) == false)
        {
            Debug.Log($"{path}が存在しませんでした");
            return null;
        }
        var reader = new StreamReader(path);
        var readString = reader.ReadToEnd();
        reader.Close();
        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);
        var loadedData = JsonConvert.DeserializeObject<T>(decryptedString);
        return loadedData;
    }
}