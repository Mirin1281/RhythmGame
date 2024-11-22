using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float holdTime = 0.5f;
    [SerializeField] int holdRepeatInterval = 3;
    CancellationTokenSource _cts;

    public event Action OnInput;
    public event Action OnHold;

    void OnDestroy()
    {
        OnInput = null;
        OnHold = null;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _cts = new();
        HoldAsync(_cts.Token).Forget();
        OnInput?.Invoke();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        _cts?.Cancel();
    }

    async UniTask HoldAsync(CancellationToken token)
    {
        float downTime = 0f;
        int i = 0;
        while (!token.IsCancellationRequested)
        {
            if (holdRepeatInterval == -1)
            {
                if (downTime > holdTime)
                {
                    OnHold?.Invoke();
                    return;
                }
            }
            else
            {
                if (downTime > holdTime && i % holdRepeatInterval == 0)
                {
                    OnInput?.Invoke();
                }
            }

            downTime += Time.deltaTime;
            i++;
            await UniTask.Yield(token);
        }
    }
}
