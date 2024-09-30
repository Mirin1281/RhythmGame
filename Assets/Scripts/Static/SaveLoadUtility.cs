using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

public class SaveLoadUtility
{
    readonly static int encryptKey = 90;

    public static async UniTask SetData(GameData saveData, string fileName)
    {
        var filePath = $"{Application.streamingAssetsPath}/{fileName}.json";
        var writer = new StreamWriter(filePath, false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
        await writer.WriteAsync(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static void SetDataImmediately(GameData saveData, string fileName)
    {
        var filePath = $"{Application.streamingAssetsPath}/{fileName}.json";
        var writer = new StreamWriter(filePath, false);
        var serializedJson = JsonConvert.SerializeObject(saveData);
        var encryptedJson = CaesarCipher.Encrypt(serializedJson, encryptKey);
         writer.Write(encryptedJson);
        writer.Flush();
        writer.Close();
    }

    public static async UniTask<GameData> GetData(string fileName, CancellationToken token = default)
    {
        var filePath = $"{Application.streamingAssetsPath}/{fileName}.json";
        var reader = new StreamReader(filePath);
        var readString = reader.ReadToEnd();
        reader.Close();

        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);

        var loadedData = await UniTask.RunOnThreadPool(() =>
            JsonConvert.DeserializeObject<GameData>(decryptedString),cancellationToken: token
        );
        return loadedData;
    }

    public static GameData GetDataImmediately(string fileName)
    {
        var filePath = $"{Application.streamingAssetsPath}/{fileName}.json";
        var reader = new StreamReader(filePath);
        var readString = reader.ReadToEnd();
        reader.Close();
        var decryptedString = CaesarCipher.Decrypt(readString, encryptKey);
        var loadedData = JsonConvert.DeserializeObject<GameData>(decryptedString);
        return loadedData;
    }
}