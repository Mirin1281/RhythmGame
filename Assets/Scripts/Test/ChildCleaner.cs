using UnityEngine;

public class ChildCleaner : MonoBehaviour
{
    [SerializeField] bool clearOnAwake = true;

    void Awake()
    {
        if(clearOnAwake)
        {
            DestroyChildren();
            Destroy(this);
        }
    }

    public void DestroyChildren()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
