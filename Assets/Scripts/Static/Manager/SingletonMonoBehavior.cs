using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool activeOnAwake;

    static T instance;
    public static T Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindAnyObjectByType<T>();

#if UNITY_EDITOR
            if (instance == null)
            {
                //Debug.LogWarning(typeof(T).Name + " is nothing");
            }
#endif
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
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