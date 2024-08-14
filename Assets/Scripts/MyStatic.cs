using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // AssetDatabase���g�����߂ɕK�v
#endif
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// �萔�Ŗ��O��ۊǂ���
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
    /// BGM�𗬂��u�Ԃ����g�p���邱��
    /// </summary>
    public static readonly float BGMMasterVolume = 1.5f;
    public static readonly float SEMasterVolume = 0.15f;

    public static readonly LayerMask WallLayer = 1 << 6;

    /*/// <summary>
    /// Tilemap�p�Az������0�ɂȂ�܂�
    /// </summary>
    public static Vector3Int ToVector3Int(Vector3 pos)
        => new Vector3Int(Mathf.RoundToInt(pos.x - 1), Mathf.RoundToInt(pos.y - 1));
    */

    /// <summary>
    /// await��Ŏw�肵���b���҂Ă܂�
    /// </summary>
    /// <param name="waitTime">�҂���(�b)</param>
    public static UniTask WaitSeconds(float waitTime)
        => UniTask.Delay(TimeSpan.FromSeconds(waitTime));

    /// <summary>
    /// �V�[�����̃R���|�[�l���g���������܂�
    /// </summary>
    /// <typeparam name="T">�R���|�[�l���g</typeparam>
    /// <param name="objName">�R���|�[�l���g�̂����I�u�W�F�N�g�̖��O(�R���|�[�l���g���Ɠ����Ȃ�ȗ���)</param>
    /// <param name="findInactive">��A�N�e�B�u����������</param>
    /// <param name="callLog">�Ă΂ꂽ�ۂɃ��O���o��</param>
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
            Log(findName + " �I�u�W�F�N�g��������܂���ł���", true);
            return default;
        }

        var component = obj.GetComponent<T>();
        if (component == null)
        {
            Log(componentName + " �R���|�[�l���g��������܂���ł���", true);
            return default;
        }
        Log(componentName + "��Find���܂���", false);

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
    /// ��A�N�e�B�u�̂��܂߂�Find���܂�
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    static GameObject FindIncludInactive(string targetName)
    {
        var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var gameObjectInHierarchy in gameObjects)
        {

#if UNITY_EDITOR
            //Hierarchy��̂��̂łȂ���΃X���[
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
