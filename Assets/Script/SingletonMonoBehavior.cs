using UnityEngine;

// シングルトンなMonoBehaviourの基底クラス
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool activeOnAwake;

    static T instance;
    public static T Instance
    {
        get
        {
            if (instance is not null) return instance;
            instance = FindObjectOfType<T>();

            if (instance is null)
            {
                Debug.LogWarning(typeof(T) + " is nothing");
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance is not null && instance != this)
        {
            Debug.LogWarning(typeof(T) + " is multiple created", this);
            return;
        }

        instance = this as T;
        gameObject.SetActive(activeOnAwake);
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            instance = null;
        }
    }
}