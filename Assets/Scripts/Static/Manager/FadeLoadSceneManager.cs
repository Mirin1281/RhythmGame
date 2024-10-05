using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class FadeLoadSceneManager : SingletonMonoBehaviour<FadeLoadSceneManager>
{
    [SerializeField] Image fadeImage;

    // フェード中ボタンの多重クリックを禁止するために使う
    public static bool IsSceneChanging { get; private set; }

    /// <summary>
    /// シーン遷移させる関数
    /// </summary>
    /// <param name="fadeInInterval">暗転の秒数</param>
    /// <param name="sceneName">遷移先のシーン名</param>
    /// <param name="fadeOutInterval">明転の秒数(省略するとフェードインと同じ)</param>
    /// <param name="fadeColor">フェードの色(デフォルトは黒)</param>
    public void LoadScene(float fadeInInterval, string sceneName, float fadeOutInterval = -1f, Color? fadeColor = null)
    {
        LoadSceneAsync(fadeInInterval, sceneName, fadeOutInterval, fadeColor).Forget();
    }

    // こっちはawaitできる
    public async UniTask LoadSceneAsync(float fadeInInterval, string sceneName, float fadeOutInterval = -1f, Color? fadeColor = null)
    {
        IsSceneChanging = true;
        if (fadeOutInterval == -1f)
        {
            fadeOutInterval = fadeInInterval;
        }

        if(fadeInInterval != 0f)
        {
            await FadeIn(fadeInInterval, fadeColor);
        }
        
        await SceneManager.LoadSceneAsync(sceneName);

        if(fadeOutInterval != 0f)
        {
            await FadeOut(fadeOutInterval, fadeColor);
        }
        IsSceneChanging = false;
    }

    public async UniTask FadeIn(float interval, Color? fadeColor = null)
    {
        fadeImage.gameObject.SetActive(true);
        var color = fadeColor ?? Color.black;
        var clearColor = new Color(color.r, color.g, color.b, 0f);
        var easing = new Easing(0f, 1f, interval, EaseType.InCubic);
        
        var time = 0f;
        while (time <= interval)
        {
            fadeImage.color = Color.Lerp(clearColor, color, easing.Ease(time));
            time += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        fadeImage.color = color;
    }

    public async UniTask FadeOut(float interval, Color? fadeColor = null)
    {
        var color = fadeColor ?? Color.black;
        var clearColor = new Color(color.r, color.g, color.b, 0f);
        var easing = new Easing(0f, 1f, interval, EaseType.InCubic);

        var time = 0f;
        while (time <= interval)
        {
            fadeImage.color = Color.Lerp(color, clearColor, easing.Ease(time));
            time += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}