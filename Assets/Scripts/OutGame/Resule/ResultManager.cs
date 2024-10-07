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
        var r = RhythmGameManager.Instance.Result;
        if(r == null) return;

        var master = r.MasterData;
        musicTitleTmpro.SetText(master.MusicData.MusicName);
        composerTmpro.SetText(master.MusicData.ComposerName);

        scoreTmpro.SetText(r.Score.ToString());

        string fumenName = r.FumenData.name;
        UniTask.Void(async () => 
        {
            int s = await GetHighScore(fumenName);
            highScoreTmpro.SetText(s.ToString("00000000"));
        });

        perfectTmpro.SetText(r.Perfect.ToString());
        greatTmpro.SetText((r.FastGreat + r.LateGreat).ToString());
        farTmpro.SetText((r.FastFar + r.LateFar).ToString());
        missTmpro.SetText(r.Miss.ToString());
        detailGreatTmpro.SetText($"F:{r.FastGreat} L:{r.LateGreat}");
        detailFarTmpro.SetText($"F:{r.FastFar} L:{r.LateFar}");

        illustratorTmpro.SetText(master.IllustratorName);
        illustImage.sprite = master.Illust;

        RhythmGameManager.Instance.MusicMasterData = r.MasterData;

        if(isSavable == false) return;
        var gameScore = new GameScore(
            fumenName,
            r.Score,
            r.IsFullCombo);
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
