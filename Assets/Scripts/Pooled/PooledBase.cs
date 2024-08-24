using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class PooledBase : MonoBehaviour
{
    bool isActiveForPool;
    bool isActive;
    public bool IsActiveForPool => isActiveForPool;
    public bool IsActive => isActive;

    void OnEnable()
    {
        isActiveForPool = true;
        isActive = true;
    }

    // プール時に2フレーム開けるとバグらない
    async UniTask OnDisable()
    {
        isActive = false;
        await UniTask.DelayFrame(2, cancellationToken: destroyCancellationToken);
        isActiveForPool = false;
    }

    public void SetActive(bool enabled)
    {
        gameObject.SetActive(enabled);
        isActive = enabled;
    }
}