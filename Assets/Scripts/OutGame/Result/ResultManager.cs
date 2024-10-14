using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [SerializeField] MusicMasterManagerData masterManagerData;
    [SerializeField] UIMover uiMover;

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

    [SerializeField] Image illustImage;
    [SerializeField] TMP_Text illustratorTmpro;

    [SerializeField] TMP_Text debugTmpro;
    
#if UNITY_EDITOR
    [SerializeField] bool isSavable;
#else
    bool isSavable = true;
#endif

    void Awake()
    {
        uiMover.MoveUI().Forget();

        var result = RhythmGameManager.Instance.Result;
        if(result == null) return;

        SetUI(result);
        if(isSavable == false) return;

        var gameScore = new GameScore(
            result.FumenData.name,
            result.Score,
            result.IsFullCombo);
        masterManagerData.SetScoreJsonAsync(gameScore).Forget();
    }

    void SetUI(Result r)
    {
        var d = r.FumenData.MusicSelectData;
        musicTitleTmpro.SetText(d.MusicName);
        composerTmpro.SetText(d.ComposerName);

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

        illustratorTmpro.SetText(d.IllustratorName);
        illustImage.sprite = d.Illust;


        // ハイスコアを譜面データの名前から取得します
        async UniTask<int> GetHighScore(string fumenName)
        {
            var list = await masterManagerData.GetScoreJsonAsync(destroyCancellationToken);     
            var score = list.FirstOrDefault(s => s.FumenName == fumenName);
            return score.Score;
        }
    }
}
