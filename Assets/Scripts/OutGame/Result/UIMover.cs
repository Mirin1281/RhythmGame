using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{
    [SerializeField] TMP_Text[] tmpros1;
    [SerializeField] TMP_Text[] tmpros2;
    [SerializeField] TMP_Text[] tmpros3;
    [SerializeField] TMP_Text[] tmpros4;
    [SerializeField] TMP_Text[] tmpros5;

    [SerializeField] TMP_Text illustratorTmpro;
    [SerializeField] Image illustImage;
    [SerializeField] Image[] Images;

    void HideUI()
    {
        foreach (var t in tmpros1)
        {
            t.alpha = 0;
        }
        foreach (var t in tmpros2)
        {
            t.alpha = 0;
        }
        foreach (var t in tmpros3)
        {
            t.alpha = 0;
        }
        foreach (var t in tmpros4)
        {
            t.alpha = 0;
        }
        foreach (var t in tmpros5)
        {
            t.alpha = 0;
        }

        illustratorTmpro.alpha = 0;
        illustImage.color = new Color(1, 1, 1, 0);

        foreach (var i in Images)
        {
            i.color = new Color(0, 0, 0, 0);
        }
    }

    public async UniTask MoveUI()
    {
        HideUI();
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        FadeAlphaAsync(illustratorTmpro, 1, 1f).Forget();
        FadeAlphaAsync(illustImage, 1, 1f).Forget();

        foreach (var t in tmpros1)
        {
            FadeAlphaAsync(t, 1, 0.4f).Forget();
            MoveAnimAsync(t, 80, 0.5f).Forget();
        }
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        foreach (var t in tmpros2)
        {
            FadeAlphaAsync(t, 1, 0.4f).Forget();
            MoveAnimAsync(t, 80, 0.5f).Forget();
        }
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        foreach (var t in tmpros3)
        {
            FadeAlphaAsync(t, 1, 0.4f).Forget();
            MoveAnimAsync(t, 40, 0.5f).Forget();
        }
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        foreach (var t in tmpros4)
        {
            FadeAlphaAsync(t, 1, 0.4f).Forget();
            MoveAnimAsync(t, 40, 0.5f).Forget();
        }
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        foreach (var t in tmpros5)
        {
            FadeAlphaAsync(t, 1, 0.4f).Forget();
            MoveAnimAsync(t, 40, 0.5f).Forget();
        }

        foreach (var i in Images)
        {
            FadeAlphaAsync(i, 1, 0.2f).Forget();
        }
    }

    async UniTask FadeAlphaAsync(TMP_Text tmpro, float toAlpha, float time)
    {
        var easing = new Easing(tmpro.color.a, toAlpha, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            tmpro.alpha = easing.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        tmpro.alpha = toAlpha;
    }
    async UniTask FadeAlphaAsync(Image image, float toAlpha, float time)
    {
        var easing = new Easing(image.color.a, toAlpha, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, easing.Ease(t));
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, toAlpha);
    }

    async UniTask MoveAnimAsync(TMP_Text tmpro, float beforeDelta, float time)
    {
        Vector3 p = tmpro.transform.localPosition;
        var easing = new Easing(p.x + beforeDelta, p.x, time, EaseType.OutQuad);

        var t = 0f;
        while (t < time)
        {
            tmpro.transform.localPosition = new Vector3(easing.Ease(t), p.y);
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        tmpro.transform.localPosition = p;
    }
}
