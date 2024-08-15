using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public abstract class PoolBase<T> : MonoBehaviour where T : PooledBase
{

    [System.Serializable]
    class PrepareStatus
    {
        [SerializeField] T prefab;
        [SerializeField, Tooltip("���O�ɐ������Ă�����")]
        int prepare;
        public int SearchIndex { get; set; }
        public T Prefab => prefab;
        public int Prepare => prepare;
    }

    [SerializeField] List<PrepareStatus> prepareStatuses;

    [SerializeField, Tooltip("���O�ɐ������Ă��������𒴉߂����ꍇ�Ƀ��O���o��")]
    bool showLog = true;

    List<List<T>> PooledTable;

    protected T GetInstance(int index = 0)
    {
        var pooledList = PooledTable[index];
        var status = prepareStatuses[index];
        var listCount = pooledList.Count;

        // ��{�͎g���܂킷
        // Prepare�𒴉߂���Ȃ�for�𔲂���
        for (int i = 0; i < listCount; i++)
        {
            if (status.SearchIndex >= listCount)
            {
                status.SearchIndex = 0;
            }
            var t = pooledList[status.SearchIndex];
            status.SearchIndex++;
            if (t.IsActiveForPool) continue; //�g�p���Ȃ玟

            t.SetActive(true);
            return t;
        }

#if UNITY_EDITOR
        if(showLog)
            Debug.Log("Prepare�𒴂���" + typeof(T).Name + "�𐶐����܂�");
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
    /// �V�����C���X�^���X�𐶐����܂�
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
