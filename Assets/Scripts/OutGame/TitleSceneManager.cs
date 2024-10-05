using UnityEngine;
using Cysharp.Threading.Tasks;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] string _sceneName;

    bool _onSceneTransition;
    public void SetTransitionFlag()
    {
        _onSceneTransition = true;
    }
    async UniTask Start()
    {
        //await MyUtility.WaitSeconds(1f, destroyCancellationToken);
        await UniTask.WaitUntil(() => _onSceneTransition, cancellationToken: destroyCancellationToken);
        if(FadeLoadSceneManager.IsSceneChanging) return;
        SEManager.Instance.PlaySE(SEType.start_freeze);
        FadeLoadSceneManager.Instance.LoadScene(0.5f, _sceneName, 0.5f, Color.white);
    }
}
