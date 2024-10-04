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
        [SerializeField, Tooltip("���O�ɐ������Ă�����")]
        int prepare;
        public int SearchIndex { get; set; }
        public T Prefab => prefab;
        public int Prepare => prepare;

        public void SetPrepare(int count)
        {
            prepare = count;
        }
    }

    [SerializeField] List<PrepareStatus> prepareStatuses;

    [SerializeField, Tooltip("���O�ɐ������Ă��������𒴉߂����ꍇ�Ƀ��O���o��")]
    bool showLog = true;

    protected List<List<T>> PooledTable;

    protected T GetInstance(int index = 0)
    {
#if UNITY_EDITOR
        // �G�f�B�^�ォ���s���łȂ����ɌĂ΂ꂽ�ۂ͐����̂ݍs��
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

    void Start()
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

    public void SetPoolCount(int count, int index = 0)
    {
        if(count < 0) return;
        prepareStatuses[index].SetPrepare(count);
    }
}
