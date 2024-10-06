using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] string _sceneName;
    [SerializeField] TMP_Text startTmpro;
    bool _onSceneTransition;

    public void SetTransitionFlag()
    {
        _onSceneTransition = true;
    }

    async UniTask Start()
    {
        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        LoopFlashTextAsync().Forget();
        await UniTask.WaitUntil(() => _onSceneTransition, cancellationToken: destroyCancellationToken);
        if(FadeLoadSceneManager.IsSceneChanging) return;
        SEManager.Instance.PlaySE(SEType.start_freeze);
        FadeLoadSceneManager.Instance.LoadScene(0.5f, _sceneName, 0.5f, Color.white);
    }

    async UniTask LoopFlashTextAsync()
    {
        float t = 0f;
        while(_onSceneTransition == false)
        {
            startTmpro.color = new Color(0, 0, 0, Mathf.Abs(Mathf.Cos(t)));
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }

        float a = startTmpro.color.a;
        t = 0f;
        while(t < 0.3f)
        {
            startTmpro.color = new Color(0, 0, 0, t.Ease(a, 0, 0.3f, EaseType.InQuad));
            t += Time.deltaTime;
            await UniTask.Yield(destroyCancellationToken);
        }
    }
}
