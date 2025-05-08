using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SettingCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup panel1;
    [SerializeField] CanvasGroup panel2;
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
    }

    public void Close()
    {
        if (isFading) return;
        UniTask.Void(async () =>
        {
            await FadeAlphaAsync(0, 0.3f, EaseType.OutCubic);
            gameObject.SetActive(false);
            canvas.enabled = false;

            // ページをリセット
            panel1.transform.localPosition = new Vector3(0, 0);
            panel2.transform.localPosition = new Vector3(1920, 0);
            panel1.gameObject.SetActive(true);
            panel2.gameObject.SetActive(false);
            panel1.alpha = 1;
            panel2.alpha = 1;
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
