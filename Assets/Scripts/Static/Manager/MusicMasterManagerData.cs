using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

/*[CreateAssetMenu(
    fileName = "MasterManager",
    menuName = "ScriptableObject/MasterManager",
    order = 10)
]*/
public class MusicMasterManagerData : ScriptableObject
{
    [Header("データを追加した後はContextMenuからスコアデータを初期化してください")]
    [SerializeField] MusicSelectData[] selectDatas;
    /// <summary>
    /// 分離してもいいかも
    /// </summary>
    public MusicSelectData[] SelectDatas => selectDatas;

    [ContextMenu("ResetScoreData")]
    public ScoreData ResetScoreData()
    {
        var scoreData = new ScoreData();
        for (int i = 0; i < selectDatas.Length; i++)
        {
            var d = selectDatas[i];
            if (d.NormalFumenReference != null)
            {
                scoreData.GameScores.Add(new GameScore(MyUtility.GetFumenName(d, Difficulty.Normal), 0, false));
            }
            if (d.HardFumenReference != null)
            {
                scoreData.GameScores.Add(new GameScore(MyUtility.GetFumenName(d, Difficulty.Hard), 0, false));
            }
            if (d.ExtraFumenReference != null)
            {
                scoreData.GameScores.Add(new GameScore(MyUtility.GetFumenName(d, Difficulty.Extra), 0, false));
            }
        }
        SaveLoadUtility.SetDataImmediately(scoreData, ConstContainer.ScoreDataName);
        Debug.Log("データをリセットしました");
        return scoreData;
    }

    /// <summary>
    /// もしスコアに更新があればJsonに保存します
    /// </summary>
    public async UniTask SetScoreJsonAsync(GameScore gameScore)
    {
        var scoreData = await SaveLoadUtility.GetDataAsync<ScoreData>(ConstContainer.ScoreDataName);
        scoreData ??= ResetScoreData();

        var s = scoreData.GameScores.FirstOrDefault(s => s.FumenName == gameScore.FumenName);
        if (s == null)
        {
            Debug.LogWarning($"{nameof(GameScore)}がnullでした\n{nameof(ScoreData)}を初期化していない可能性があります");
            Debug.LogWarning($"FumenAddress: {gameScore.FumenName}");
            return;
        }

        int score = s.Score;
        if (s.Score < gameScore.Score)
        {
            score = gameScore.Score;
        }
        bool isFullCombo = s.IsFullCombo;
        if (isFullCombo == false && gameScore.IsFullCombo)
        {
            isFullCombo = gameScore.IsFullCombo;
        }

        s.UpdateScore(score, isFullCombo);
        await SaveLoadUtility.SetDataAsync(scoreData, ConstContainer.ScoreDataName);
        Debug.Log("データが保存されました");
    }

    /// <summary>
    /// Json内のスコアリストを取得します
    /// </summary>
    public async UniTask<List<GameScore>> GetScoreJsonAsync(CancellationToken token)
    {
        var scoreData = await SaveLoadUtility.GetDataAsync<ScoreData>(ConstContainer.ScoreDataName, token);
        scoreData ??= ResetScoreData();
        if (scoreData == null || scoreData.GameScores == null)
        {
            Debug.LogError("スコアデータの取得で問題が発生しました");
        }
        return scoreData.GameScores;
    }
}

/// <summary>
/// 1譜面に1つのスコアを保持する
/// </summary>
[Serializable]
public class GameScore
{
    readonly string fumenName;
    int score;
    bool isFullCombo;

    public string FumenName => fumenName;
    public int Score => score;
    public bool IsFullCombo => isFullCombo;

    public GameScore(string fumenName, int score, bool isFullCombo)
    {
        this.fumenName = fumenName;
        this.score = score;
        this.isFullCombo = isFullCombo;
    }

    public void UpdateScore(int score, bool isFullCombo)
    {
        this.score = score;
        this.isFullCombo = isFullCombo;
    }
}