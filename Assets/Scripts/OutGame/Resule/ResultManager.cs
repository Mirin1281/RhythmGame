using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [SerializeField] MusicMasterManagerData masterManagerData;

    [SerializeField] TMP_Text musicTitleTmpro;
    [SerializeField] TMP_Text composerTmpro;

    [SerializeField] TMP_Text scoreTmpro;
    [SerializeField] TMP_Text highScoreTmpro;

    [SerializeField] TMP_Text perfectTmpro;
    [SerializeField] TMP_Text greatTmpro;
    [SerializeField] TMP_Text detailGreatTmpro;
    [SerializeField] TMP_Text farTmpro;
    [SerializeField] TMP_Text detailFarTmpro;
    [SerializeField] TMP_Text missTmpro;

    [SerializeField] TMP_Text illustratorTmpro;
    [SerializeField] Image illustImage;
    
#if UNITY_EDITOR
    [SerializeField] bool isSavable;
#else
    bool isSavable = true;
#endif

    void Awake()
    {
        var result = RhythmGameManager.Instance.Result;
        if(result == null) return;

        musicTitleTmpro.SetText(result.MasterData.MusicData.MusicName);
        composerTmpro.SetText(result.MasterData.MusicData.ComposerName);

        scoreTmpro.SetText(result.Score.ToString());

        string fumenName = result.MasterData.GetFumenData(RhythmGameManager.Difficulty).name;
        UniTask.Void(async () => 
        {
            int s = await GetHighScore(fumenName);
            highScoreTmpro.SetText(s.ToString("00000000"));
        });

        perfectTmpro.SetText(result.Perfect.ToString());
        greatTmpro.SetText((result.FastGreat + result.LateGreat).ToString());
        farTmpro.SetText((result.FastFar + result.LateFar).ToString());
        missTmpro.SetText(result.Miss.ToString());
        detailGreatTmpro.SetText($"F:{result.FastGreat} L:{result.LateGreat}");
        detailFarTmpro.SetText($"F:{result.FastFar} L:{result.LateFar}");

        illustratorTmpro.SetText(result.MasterData.IllustratorName);
        illustImage.sprite = result.MasterData.Illust;

        RhythmGameManager.Instance.MusicMasterData = result.MasterData;

        if(isSavable == false) return;
        
        var gameScore = new GameScore(
            fumenName,
            result.Score,
            result.IsFullCombo);
        masterManagerData.SetScoreJsonAsync(gameScore).Forget();
    }

    /// <summary>
    /// ハイスコアを譜面データの名前から取得します
    /// </summary>
    async UniTask<int> GetHighScore(string fumenName)
    {
        var list = await masterManagerData.GetScoreJsonAsync(destroyCancellationToken);     
        var score = list.FirstOrDefault(s => s.FumenName == fumenName);
        return score.Score;
    }
}
