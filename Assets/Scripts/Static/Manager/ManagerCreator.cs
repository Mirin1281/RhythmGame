using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ManagerCreator : SingletonMonoBehaviour<ManagerCreator>
{
    [Serializable]
    class CreateManagerStatuses
    {
        [field: SerializeField]
        public GameObject ManagerPrefab { get; private set; }

        [field: SerializeField, Tooltip("生成時に非アクティブにします")]
        public bool IsInactiveOnAwake { get; private set; }
    }

    [SerializeField] CreateManagerStatuses[] managerStatuses;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeAwake()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(nameof(ManagerCreator));
        GameObject managerPrefab = handle.WaitForCompletion(); // 同期的にロード
        ManagerCreator instance = Instantiate(managerPrefab).GetComponent<ManagerCreator>();

        if (managerPrefab == null)
        {
            Debug.LogWarning($"{nameof(ManagerCreator)}の取得に失敗しました");
            return;
        }
        instance.name = managerPrefab.name;
        DontDestroyOnLoad(instance);
        instance.InitCreateManagers();
        Addressables.Release(handle);
    }

    public void InitCreateManagers()
    {
        foreach (var status in managerStatuses)
        {
            var obj = Instantiate(status.ManagerPrefab);
            obj.name = status.ManagerPrefab.name;
            obj.transform.SetParent(this.transform);
            if (status.IsInactiveOnAwake)
            {
                obj.SetActive(false);
            }
        }
        Destroy(this);
    }
}
