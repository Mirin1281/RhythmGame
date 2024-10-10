using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ダブルタップで反応するボタン
public class PauseButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] float allowInterval = 1.5f;
    [SerializeField] UnityEvent unityEvent;
    bool isClicked;

    public event Action OnInput;

    void OnDestroy()
    {
        OnInput = null;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if(isClicked == false)
        {
            EnableAsync().Forget();
        }
        else
        {
            OnInput?.Invoke();
            unityEvent?.Invoke();
        }
    }

    async UniTask EnableAsync()
    {
        float downTime = 0f;
        while(downTime < allowInterval)
        {
            isClicked = true;
            downTime += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        isClicked = false;
    }
}
