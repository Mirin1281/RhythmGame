using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SettingCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup canvasGroup;

    public void Open()
    {
        gameObject.SetActive(true);
        canvas.enabled = true;
        FadeAlphaAsync(1, 0.3f, EaseType.OutCubic).Forget();
    }

    public void Close()
    {
        UniTask.Void(async () => 
        {
            await FadeAlphaAsync(0, 0.3f, EaseType.OutCubic);
            gameObject.SetActive(false);
            canvas.enabled = false;
        });
    }

    public void Toggle()
    {
        if(gameObject.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    async UniTask FadeAlphaAsync(float toAlpha, float time, EaseType easeType)
    {
        var outQuad = new Easing(canvasGroup.alpha, toAlpha, time, easeType);
        var t = 0f;
        while (t < time)
        {
            canvasGroup.alpha = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        canvasGroup.alpha = toAlpha;
    }
}
