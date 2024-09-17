using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] bool isUp;
    [SerializeField] float holdTime = 0.5f;
    [SerializeField] int holdRepeatInterval = 3;
    CancellationTokenSource _cts;

    public event Action<bool> OnInput;

    void OnDestroy()
    {
        OnInput = null;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _cts = new();
        HoldAsync(_cts.Token).Forget();
        OnInput?.Invoke(isUp);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        _cts?.Cancel();
    }

    async UniTask HoldAsync(CancellationToken token)
    {
        float downTime = 0f;
        int i = 0;
        while(!token.IsCancellationRequested)
        {
            if(downTime > holdTime && i % holdRepeatInterval == 0)
            {
                OnInput?.Invoke(isUp);
            }
            downTime += Time.deltaTime;
            i++;
            await UniTask.Yield(token);
        }
    }
}
