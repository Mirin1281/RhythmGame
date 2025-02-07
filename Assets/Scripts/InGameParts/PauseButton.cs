using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ダブルタップで反応するボタン
public class PauseButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Image image;
    [SerializeField] float allowInterval = 1.5f;
    [SerializeField] UnityEvent unityEvent;
    [SerializeField] Sprite oneTapSprite;
    Sprite tmpSprite;
    bool isClicked;

    public event Action OnInput;

    void OnDestroy()
    {
        OnInput = null;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        CancellationTokenSource cts = new();
        cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, destroyCancellationToken);
        if (isClicked == false)
        {
            EnableAsync(cts.Token).Forget();
        }
        else
        {
            OnInput?.Invoke();
            unityEvent?.Invoke();
            cts.Cancel();
            image.sprite = tmpSprite;
        }
    }

    async UniTask EnableAsync(CancellationToken token)
    {
        tmpSprite = image.sprite;
        if (oneTapSprite != null) image.sprite = oneTapSprite;

        float downTime = 0f;
        while (downTime < allowInterval)
        {
            isClicked = true;
            downTime += Time.deltaTime;
            await UniTask.Yield(token);
        }
        isClicked = false;

        image.sprite = tmpSprite;
    }
}
