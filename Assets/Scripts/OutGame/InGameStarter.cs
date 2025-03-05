using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class InGameStarter : MonoBehaviour
{
    [System.Serializable]
    class MoveObject
    {
        [field: SerializeField] public GameObject Obj { get; private set; }
        [field: SerializeField] public float MoveX { get; private set; }
    }

    [SerializeField] MusicPreviewer previewer;
    [SerializeField] MoveObject[] moveObjects;
    [SerializeField] Image jacketImage;

    public void StartGame(MusicSelectData selectData, Difficulty difficulty = Difficulty.None)
    {
        Debug.Log($"楽曲名: {selectData.MusicName}\n" +
            $"難易度: {RhythmGameManager.Difficulty} {selectData.GetFumenLevel(RhythmGameManager.Difficulty)}");
        RhythmGameManager.FumenReference = selectData.GetFumenReference(difficulty);
        if (difficulty != Difficulty.None)
        {
            RhythmGameManager.Difficulty = difficulty;
        }
        previewer.Stop(0.5f).Forget();

        for (int i = 0; i < moveObjects.Length; i++)
        {
            var moveObj = moveObjects[i];
            moveObj.Obj.transform.DOLocalMoveX(moveObj.MoveX, 0.5f).SetRelative(true).SetEase(Ease.OutQuart);
        }

        var easing = new Easing(jacketImage.rectTransform.sizeDelta.x, 700, 0.3f, EaseType.OutQuad);
        easing.EaseAsync(destroyCancellationToken, 0, t => jacketImage.rectTransform.sizeDelta = new Vector2(t, t)).Forget();

        UniTask.Void(async () =>
        {
            EventSystem.current.enabled = false;
            await MyUtility.WaitSeconds(1f, destroyCancellationToken);
            await SceneManager.LoadSceneAsync(ConstContainer.InGameSceneName, LoadSceneMode.Additive);
            var inGameScene = SceneManager.GetSceneByName(ConstContainer.InGameSceneName);
            var obj = inGameScene.GetRootGameObjects()[0];
            Debug.Log(obj.name);
        });


        //FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f);

    }

    public void StartGame(AssetReference reference)
    {
        RhythmGameManager.FumenReference = reference;
        previewer.Stop(0.5f).Forget();
        FadeLoadSceneManager.Instance.LoadScene(0.5f, ConstContainer.InGameSceneName, 0.5f);
    }
}
