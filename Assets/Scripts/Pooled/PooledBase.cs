using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// プール時に使用するアクティブの情報をもつ
/// </summary>
public abstract class PooledBase : MonoBehaviour
{
    bool isActiveForPool;
    public bool IsActiveForPool => isActiveForPool;
    public bool IsActive => gameObject.activeInHierarchy;

    void OnEnable()
    {
        isActiveForPool = true;
    }

    // プール時に2フレーム開けるとバグらない
    async UniTaskVoid OnDisable()
    {
        await UniTask.DelayFrame(2, cancellationToken: destroyCancellationToken);
        isActiveForPool = false;
    }

    public void SetActive(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}

public interface ITransformable
{
    public Vector3 GetPos(bool isWorld = false);
    public void SetPos(Vector3 pos);
    public void SetRotate(float deg);
}