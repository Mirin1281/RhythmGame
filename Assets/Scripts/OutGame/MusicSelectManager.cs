using UnityEngine;
using Cysharp.Threading.Tasks;
using NoteCreating;
using UnityEngine.AddressableAssets;

public class MusicSelectManager : MonoBehaviour
{
    [SerializeField] MusicButtonManager musicButtonManager;
    [SerializeField] DifficultyGroup difficultyGroup;
    [SerializeField] MusicPreviewer previewer;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        musicButtonManager.Init().Forget();
        difficultyGroup.Init();
    }

    public void StartGame(MusicSelectData selectData, Difficulty difficulty = Difficulty.None)
    {
        RhythmGameManager.FumenReference = selectData.GetFumenReference(difficulty);
        if (difficulty != Difficulty.None)
        {
            RhythmGameManager.Difficulty = difficulty;
        }
        previewer.Stop(0.5f).Forget();
        FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
        Debug.Log($"楽曲名: {selectData.MusicName}\n" +
            $"難易度: {RhythmGameManager.Difficulty} {selectData.GetFumenLevel(RhythmGameManager.Difficulty)}");
    }

    public void StartGame(AssetReference reference)
    {
        RhythmGameManager.FumenReference = reference;
        previewer.Stop(0.5f).Forget();
        FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
    }
}
