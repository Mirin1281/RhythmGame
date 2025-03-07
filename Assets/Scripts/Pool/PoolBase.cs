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
            [SerializeField, Tooltip("事前に生成しておく数")]
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
        [SerializeField, Tooltip("事前に生成しておいた数を超過した場合にログを出す")]
        bool showLog = true;
#endif

        protected List<List<T>> PooledTable;


        protected T GetInstance(int index = 0)
        {
#if UNITY_EDITOR
            // エディタ上かつ実行中でない時に呼ばれた際は生成のみ行う
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                var t = Instantiate(prepareStatuses[index].Prefab);
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
            if (showLog)
                Debug.Log($"Prepareを超えて{typeof(T).Name}({index})を生成します");
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
            if (poolCount == null || poolCount.Length == 0 || poolCount[0] == -1)
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
        [Tooltip("実行中最も多くプールされた数を記録します\n" +
            "PoolManagerの\"Apply PoolCount\"を実行することで譜面データに使用数を反映させることができます")]
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