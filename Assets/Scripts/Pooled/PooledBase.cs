using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// プール時に使用するアクティブの情報をもつ
/// </summary>
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

public interface ITransformable
{
    public Vector3 GetPos(bool isWorld = false);
    public void SetPos(Vector3 pos);

    public void SetRotate(float deg);
}