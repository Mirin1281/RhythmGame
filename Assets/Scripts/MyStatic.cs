using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // AssetDatabaseを使うために必要
#endif
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// 定数で名前を保管する
/// </summary>
public static class NameContainer
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    public const string Magic = "Magic";
}

public static class MyStatic
{
    /// <summary>
    /// BGMを流す瞬間だけ使用すること
    /// </summary>
    public static readonly float BGMMasterVolume = 1.5f;
    public static readonly float SEMasterVolume = 0.15f;

    public static readonly LayerMask WallLayer = 1 << 6;

    /*/// <summary>
    /// Tilemap用、z成分は0になります
    /// </summary>
    public static Vector3Int ToVector3Int(Vector3 pos)
        => new Vector3Int(Mathf.RoundToInt(pos.x - 1), Mathf.RoundToInt(pos.y - 1));
    */

    /// <summary>
    /// await句で指定した秒数待てます
    /// </summary>
    /// <param name="waitTime">待つ時間(秒)</param>
    public static UniTask WaitSeconds(float waitTime)
        => UniTask.Delay(TimeSpan.FromSeconds(waitTime));

    /// <summary>
    /// シーン内のコンポーネントを検索します
    /// </summary>
    /// <typeparam name="T">コンポーネント</typeparam>
    /// <param name="objName">コンポーネントのついたオブジェクトの名前(コンポーネント名と同じなら省略可)</param>
    /// <param name="findInactive">非アクティブも検索する</param>
    /// <param name="callLog">呼ばれた際にログを出す</param>
    /// <returns></returns>
    public static T FindComponent<T>(
        string objName = null, bool findInactive = true, bool callLog = true)
    {
#if UNITY_EDITOR
#else
        callLog = false;
#endif
        var componentName = typeof(T).Name;
        var findName = objName ?? componentName;
        var obj = GameObject.Find(findName);
        if (obj == null && findInactive)
        {
            obj = FindIncludInactive(findName);
        }
        if (obj == null)
        {
            Log(findName + " オブジェクトが見つかりませんでした", true);
            return default;
        }

        var component = obj.GetComponent<T>();
        if (component == null)
        {
            Log(componentName + " コンポーネントが見つかりませんでした", true);
            return default;
        }
        Log(componentName + "をFindしました", false);

        return component;


        void Log(string str, bool isWarning)
        {
            if (callLog == false) return;
            if (isWarning)
            {
                Debug.LogWarning("<color=red>" + str + "</color>");
            }
            else
            {
                Debug.Log("<color=lightblue>" + str + "</color>");
            }
        }
    }

    /// <summary>
    /// 非アクティブのも含めてFindします
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    static GameObject FindIncludInactive(string targetName)
    {
        var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var gameObjectInHierarchy in gameObjects)
        {

#if UNITY_EDITOR
            //Hierarchy上のものでなければスルー
            if (!AssetDatabase.GetAssetOrScenePath(gameObjectInHierarchy).Contains(".unity"))
            {
                continue;
            }
#endif
            if (gameObjectInHierarchy.name == targetName)
            {
                return gameObjectInHierarchy;
            }
        }
        return null;
    }
}
