using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class FadeLoadSceneManager : SingletonMonoBehaviour<FadeLoadSceneManager>
{
    [SerializeField] Image fadeImage;

    // �t�F�[�h���{�^���̑��d�N���b�N���֎~���邽�߂Ɏg��
    public static bool IsSceneChanging { get; private set; }

    public Color FadeColor = Color.white;

    /// <summary>
    /// �V�[���J�ڂ�����֐�
    /// </summary>
    /// <param name="fadeInInterval">�Ó]�̕b��</param>
    /// <param name="sceneName">�J�ڐ�̃V�[����</param>
    /// <param name="fadeOutInterval">���]�̕b��(�ȗ�����ƃt�F�[�h�C���Ɠ���)</param>
    /// <param name="fadeColor">�t�F�[�h�̐F(�f�t�H���g�͍�)</param>
    public void LoadScene(float fadeInInterval, string sceneName, float fadeOutInterval = -1f, Color? fadeColor = null)
    {
        LoadSceneAsync(fadeInInterval, sceneName, fadeOutInterval, fadeColor).Forget();
    }

    // ��������await�ł���
    public async UniTask LoadSceneAsync(float fadeInInterval, string sceneName, float fadeOutInterval = -1f, Color? fadeColor = null)
    {
        IsSceneChanging = true;
        if (fadeOutInterval == -1f)
        {
            fadeOutInterval = fadeInInterval;
        }

        if (fadeInInterval != 0f)
        {
            await FadeIn(fadeInInterval, fadeColor);
        }

        await SceneManager.LoadSceneAsync(sceneName);

        if (fadeOutInterval != 0f)
        {
            await FadeOut(fadeOutInterval, fadeColor);
        }
        IsSceneChanging = false;
    }

    public async UniTask FadeIn(float interval, Color? fadeColor = null)
    {
        fadeImage.gameObject.SetActive(true);
        var color = fadeColor ?? FadeColor;
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
        var color = fadeColor ?? FadeColor;
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