using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PoolBase<T> : MonoBehaviour where T : PooledBase
{

    [System.Serializable]
    class PrepareStatus
    {
        [SerializeField] T prefab;
        [SerializeField, Tooltip("事前に生成しておく数")]
        int prepare;
        public int SearchIndex { get; set; }
        public T Prefab => prefab;
        public int Prepare => prepare;
    }

    [SerializeField] List<PrepareStatus> prepareStatuses;

    [SerializeField, Tooltip("事前に生成しておいた数を超過した場合にログを出す")]
    bool showLog = true;

    protected List<List<T>> PooledTable;

    protected T GetInstance(int index = 0)
    {
#if UNITY_EDITOR
        // エディタ上かつ実行中でない時に呼ばれた際は生成のみ行う
        if(EditorApplication.isPlaying == false)
        {
            var t = GameObject.Instantiate(prepareStatuses[0].Prefab);
            t.SetActive(true);  
            return t;
        }
#endif
        
        var pooledList = PooledTable[index];
        var status = prepareStatuses[index];
        var listCount = pooledList.Count;

        // 基本は使いまわす
        // Prepareを超過するならforを抜ける
        for (int i = 0; i < listCount; i++)
        {
            if (status.SearchIndex >= listCount)
            {
                status.SearchIndex = 0;
            }
            var t = pooledList[status.SearchIndex];
            status.SearchIndex++;
            if (t.IsActiveForPool) continue; //使用中なら次

            t.SetActive(true);
            return t;
        }

#if UNITY_EDITOR
        if(showLog)
            Debug.Log("Prepareを超えて" + typeof(T).Name + "を生成します");
#endif
        return NewInstantiate(status, true);
    }

    void Awake()
    {
        PooledTable = new List<List<T>>(prepareStatuses.Count);
        for(int i = 0; i < prepareStatuses.Count; i++)
        {
            PooledTable.Add(new List<T>(prepareStatuses[i].Prepare));
        }
        StartInstance();
    }

    void StartInstance()
    {
        for (int i = 0; i < prepareStatuses.Count; i++)
        {
            for (int j = 0; j < prepareStatuses[i].Prepare; j++)
            {
                NewInstantiate(prepareStatuses[i], false);
            }
        }
    }

    /// <summary>
    /// 新しくインスタンスを生成します
    /// </summary>
    T NewInstantiate(PrepareStatus status, bool isActive)
    {
        var t = Instantiate(status.Prefab, transform);
        t.SetActive(isActive);        
        var list = PooledTable[prepareStatuses.IndexOf(status)];
        list.Add(t);
        return t;
    }
}
