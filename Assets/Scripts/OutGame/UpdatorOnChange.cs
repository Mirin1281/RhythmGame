using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using System.Collections.Generic;
using System.Linq;

// 曲選択画面で変更があった際に画面を更新する
public class UpdatorOnChange : MonoBehaviour
{
    [SerializeField] MusicMasterManagerData masterManagerData;
    [SerializeField] MusicButtonManager musicButtonManager;
    [SerializeField] DifficultyGroup difficultyGroup;
    [SerializeField] MusicPreviewer previewer;

    [SerializeField] TMP_Text titleTmpro;
    [SerializeField] TMP_Text composerTmpro;
    [SerializeField] Image illustImage;
    [SerializeField] TMP_Text illustratorTmpro;
    [SerializeField] TMP_Text highScoreTmpro;
    List<GameScore> scores;
    MusicMasterData selectedMasterData;
    string selectedFumenName;

    void Awake()
    {
        musicButtonManager.OnOtherSelect += UpdateUIAndMusic;
        difficultyGroup.OnChangeDifficulty += UpdateHighScore;
        scores = null;
    }

    // 曲が変更された際に呼ばれる
    void UpdateUIAndMusic(MusicMasterData data)
    {
        previewer.Stop(0f);
        previewer.MusicPreview(data.MusicData).Forget();
        titleTmpro.SetText(data.MusicData.MusicName);
        composerTmpro.SetText(data.MusicData.ComposerName);
        illustImage.sprite = data.Illust;
        illustratorTmpro.SetText(data.IllustratorName);
        selectedMasterData = data;
        selectedFumenName = data.GetFumenData(RhythmGameManager.Difficulty).name;
        SEManager.Instance.PlaySE(SEType.my1);
        SetHighScoreText(selectedFumenName).Forget();
    }

    // 難易度が変更された際に呼ばれる
    void UpdateHighScore(Difficulty difficulty)
    {
        selectedFumenName = selectedMasterData.GetFumenData(RhythmGameManager.Difficulty).name;
        SEManager.Instance.PlaySE(SEType.ti);
        SetHighScoreText(selectedFumenName).Forget();
    }

    async UniTask SetHighScoreText(string fumenName)
    {
        var list = await GetGameScoresAsync();        
        var gameScore = list.FirstOrDefault(s => s.FumenName == fumenName);
        (int highScore, bool isFullCombo) = (gameScore.Score, gameScore.IsFullCombo);
        string fullComboText = isFullCombo ? "[F]" : string.Empty;
        highScoreTmpro.SetText($"{fullComboText} {highScore:00000000}");
    }

    /// <summary>
    /// Jsonから記録を取得します。初回以降はキャッシュされます
    /// </summary>
    async UniTask<List<GameScore>> GetGameScoresAsync()
    {
        scores ??= await masterManagerData.GetScoreJsonAsync(destroyCancellationToken);
        return scores;
    }
}
