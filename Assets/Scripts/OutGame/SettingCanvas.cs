using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SettingCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image negativeImage;
    bool isFading;

    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (isFading) return;
        gameObject.SetActive(true);
        canvas.enabled = true;
        FadeAlphaAsync(1, 0.3f, EaseType.OutCubic).Forget();
        if (negativeImage != null)
            negativeImage.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (isFading) return;
        UniTask.Void(async () =>
        {
            await FadeAlphaAsync(0, 0.3f, EaseType.OutCubic);
            gameObject.SetActive(false);
            canvas.enabled = false;
            if (negativeImage != null)
                negativeImage.gameObject.SetActive(false);
        });
    }

    async UniTask FadeAlphaAsync(float toAlpha, float time, EaseType easeType)
    {
        isFading = true;
        var outQuad = new Easing(canvasGroup.alpha, toAlpha, time, easeType);
        var t = 0f;
        while (t < time)
        {
            canvasGroup.alpha = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        canvasGroup.alpha = toAlpha;
        isFading = false;
    }
}
