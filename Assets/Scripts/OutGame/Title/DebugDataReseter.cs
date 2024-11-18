using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class DebugDataReseter : MonoBehaviour
{
    [SerializeField] MusicMasterManagerData managerData;

#if UNITY_EDITOR
    int touchCount = 2;
#else
    int touchCount = 4;
#endif

    void Awake()
    {
        EnhancedTouchSupport.Enable();
        ResetScore().Forget();
        ResetManagerData().Forget();
    }
    void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    async UniTask ResetScore()
    {
        while (true)
        {
            var fingers = Touch.activeFingers;
            if (fingers.Count >= touchCount)
            {
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].screenPosition.x > ConstContainer.ScreenSize.x / 2f) return;
                }
                managerData.ResetScoreData();
                FadeLoadSceneManager.Instance.FadeIn(0, fadeColor: Color.black).Forget();
                await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
                FadeLoadSceneManager.Instance.FadeOut(0, fadeColor: Color.black).Forget();
                return;
            }
            await UniTask.Yield(destroyCancellationToken);
        }
    }

    async UniTask ResetManagerData()
    {
        while (true)
        {
            var fingers = Touch.activeFingers;
            if (fingers.Count >= touchCount)
            {
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].screenPosition.x < ConstContainer.ScreenSize.x / 2f) return;
                }
                RhythmGameManager.Instance.ResetData();
                FadeLoadSceneManager.Instance.FadeIn(0, fadeColor: Color.black).Forget();
                await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
                FadeLoadSceneManager.Instance.FadeOut(0, fadeColor: Color.black).Forget();
                return;
            }
            await UniTask.Yield(destroyCancellationToken);
        }
    }
}
