using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public abstract class PoolBase<T> : MonoBehaviour where T : MonoBehaviour, IPoolable
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
        [SerializeField] bool initOnStart;

#if UNITY_EDITOR
        [SerializeField, Tooltip("���O�ɐ������Ă��������𒴉߂����ꍇ�Ƀ��O���o��")]
        bool showLog = true;
#endif

        protected List<List<T>> PooledTable;


        protected T GetInstance(int index = 0)
        {
#if UNITY_EDITOR
            // �G�f�B�^�ォ���s���łȂ����ɌĂ΂ꂽ�ۂ͐����̂ݍs��
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                var t = Instantiate(prepareStatuses[0].Prefab);
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
            if (showLog)
                Debug.Log("Prepare�𒴂���" + typeof(T).Name + "�𐶐����܂�");
#endif
            return NewInstantiate(status, true);
        }

        void Start()
        {
            if (initOnStart)
                Init();
        }

        public void Init(params int[] poolCount)
        {
            PooledTable = new List<List<T>>(prepareStatuses.Count);
            if (poolCount == null || poolCount.Length == 0)
            {
                for (int i = 0; i < prepareStatuses.Count; i++)
                {
                    PooledTable.Add(new List<T>(prepareStatuses[i].Prepare));
                }
            }
            else
            {
                for (int i = 0; i < prepareStatuses.Count; i++)
                {
                    PooledTable.Add(new List<T>(poolCount[i]));
                }
            }
            StartInstance(poolCount);
        }

        void StartInstance(int[] poolCount)
        {
            for (int i = 0; i < prepareStatuses.Count; i++)
            {
                if (poolCount == null || poolCount.Length == 0)
                {
                    for (int j = 0; j < prepareStatuses[i].Prepare; j++)
                    {
                        NewInstantiate(prepareStatuses[i], false);
                    }
                }
                else
                {
                    for (int j = 0; j < poolCount[i]; j++)
                    {
                        NewInstantiate(prepareStatuses[i], false);
                    }
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

        public IEnumerable<T> GetInstances(int index = 0, bool isExtractInactive = true)
        {
            var list = PooledTable[index];
            if (isExtractInactive)
            {
                return list.Where(t => t.IsActive);
            }
            else
            {
                return list;
            }
        }

#if UNITY_EDITOR
        [Tooltip("���s���ł������v�[�����ꂽ�����L�^���܂�\n" +
            "PoolManager��\"Apply PoolCount\"�����s���邱�Ƃŕ��ʃf�[�^�Ɏg�p���𔽉f�����邱�Ƃ��ł��܂�")]
        public int MaxUseCount;

        void Awake()
        {
            LoopCheckPoolCount().Forget();
        }
        async UniTaskVoid LoopCheckPoolCount()
        {
            await MyUtility.WaitSeconds(0.5f, destroyCancellationToken);
            while (this)
            {
                if (PooledTable == null) return;
                var pooledList = PooledTable[0];
                if (pooledList == null) return;
                int count = pooledList.Count(t => t.IsActiveForPool);
                if (MaxUseCount < count)
                {
                    MaxUseCount = count;
                }
                await UniTask.Yield(destroyCancellationToken);
            }
        }
#endif
    }
}