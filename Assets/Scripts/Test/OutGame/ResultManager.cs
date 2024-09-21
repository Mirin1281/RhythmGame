using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
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

    void Awake()
    {
        var result = RhythmGameManager.Instance.Result;
        if(result == null) return;

        musicTitleTmpro.SetText(result.MasterData.MusicData.MusicName);
        composerTmpro.SetText(result.MasterData.MusicData.ComposerName);

        scoreTmpro.SetText(result.Score.ToString());
        //highScoreTmpro.SetText();

        perfectTmpro.SetText(result.Perfect.ToString());
        greatTmpro.SetText((result.FastGreat + result.LateGreat).ToString());
        farTmpro.SetText((result.FastFar + result.LateFar).ToString());
        missTmpro.SetText(result.Miss.ToString());
        detailGreatTmpro.SetText($"F:{result.FastGreat} L:{result.LateGreat}");
        detailFarTmpro.SetText($"F:{result.FastFar} L:{result.LateFar}");

        illustratorTmpro.SetText($"Illust: {result.MasterData.IllustratorName}");
        illustImage.sprite = result.MasterData.Illust;

        RhythmGameManager.Instance.MusicMasterData = result.MasterData;
    }
}
