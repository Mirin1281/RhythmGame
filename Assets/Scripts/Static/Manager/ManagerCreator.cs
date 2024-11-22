using System;
using UnityEngine;

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
        var managerPrefab = Resources.Load<ManagerCreator>(nameof(ManagerCreator));
        if (managerPrefab == null)
        {
            Debug.LogWarning($"{nameof(ManagerCreator)}の取得に失敗しました");
            return;
        }
        var self = Instantiate(managerPrefab);
        self.name = managerPrefab.name;
        DontDestroyOnLoad(self);
        self.InitCreateManagers();
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
