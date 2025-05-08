using Cysharp.Threading.Tasks;
using UnityEngine;

// 汎用性は全く考えていません
public class PageTransitioner : MonoBehaviour
{
    [SerializeField] CanvasGroup panel1;
    [SerializeField] CanvasGroup panel2;

    float easeTime = 0.2f;
    const float chank = 1920;

    public void TransitionPage(int toPageIndex)
    {
        UniTask.Void(async () =>
        {
            if (toPageIndex == 1) // ページ2からページ1に遷移するとする
            {
                FadeAlphaAsync(panel1, 1).Forget();
                await FadeAlphaAsync(panel2, 0);
                ShowPanelAsync(panel1.gameObject, chank).Forget();
                HidePanelAsync(panel2.gameObject, chank).Forget();
            }
            else if (toPageIndex == 2)
            {
                FadeAlphaAsync(panel2, 1).Forget();
                await FadeAlphaAsync(panel1, 0);
                ShowPanelAsync(panel2.gameObject, -chank).Forget();
                HidePanelAsync(panel1.gameObject, -chank).Forget();
            }
        });
    }

    UniTask FadeAlphaAsync(CanvasGroup panel, float endAlpha)
    {
        var easing = new Easing(panel.alpha, endAlpha, easeTime, EaseType.OutQuad);
        return easing.EaseAsync(destroyCancellationToken, 0, v =>
        {
            panel.alpha = v;
        });
    }

    async UniTask ShowPanelAsync(GameObject panel, float moveX)
    {
        panel.SetActive(true);
        float endX = panel.transform.localPosition.x + moveX;
        var xEasing = new Easing(panel.transform.localPosition.x, endX, easeTime, EaseType.OutQuad);
        float t = 0;
        while (t < easeTime)
        {
            panel.transform.localPosition = new Vector3(xEasing.Ease(t), 0);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        panel.transform.localPosition = new Vector3(endX, 0);
    }

    async UniTask HidePanelAsync(GameObject panel, float moveX)
    {
        float endX = panel.transform.localPosition.x + moveX;
        var xEasing = new Easing(panel.transform.localPosition.x, panel.transform.localPosition.x + moveX, easeTime, EaseType.OutQuad);
        float t = 0;
        while (t < easeTime)
        {
            panel.transform.localPosition = new Vector3(xEasing.Ease(t), 0);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        panel.transform.localPosition = new Vector3(endX, 0);
        panel.SetActive(false);
    }
}
