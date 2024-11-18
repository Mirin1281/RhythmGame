using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

[CreateAssetMenu(
    fileName = "MasterManager",
    menuName = "ScriptableObject/MasterManager",
    order = 4)
]
public class MusicMasterManagerData : ScriptableObject
{
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
            if (!string.IsNullOrEmpty(d.NormalFumenAddress))
            {
                scoreData.GameScores.Add(new GameScore(d.NormalFumenAddress, 0, false));
            }
            if (!string.IsNullOrEmpty(d.HardFumenAddress))
            {
                scoreData.GameScores.Add(new GameScore(d.HardFumenAddress, 0, false));
            }
            if (!string.IsNullOrEmpty(d.ExtraFumenAddress))
            {
                scoreData.GameScores.Add(new GameScore(d.ExtraFumenAddress, 0, false));
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
        var scoreData = await SaveLoadUtility.GetData<ScoreData>(ConstContainer.ScoreDataName);
        scoreData ??= ResetScoreData();

        var s = scoreData.GameScores.FirstOrDefault(s => s.FumenAddress == gameScore.FumenAddress);

        if (s == null)
        {
            Debug.LogWarning($"{nameof(GameScore)}がnullでした\n{nameof(ScoreData)}を初期化していない可能性があります");
            Debug.LogWarning($"FumenAddress: {gameScore.FumenAddress}");
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
        await SaveLoadUtility.SetData(scoreData, ConstContainer.ScoreDataName);
        Debug.Log("データが保存されました");
    }

    /// <summary>
    /// Json内のスコアリストを取得します
    /// </summary>
    public async UniTask<List<GameScore>> GetScoreJsonAsync(CancellationToken token)
    {
        var scoreData = await SaveLoadUtility.GetData<ScoreData>(ConstContainer.ScoreDataName, token);
        scoreData ??= ResetScoreData();
        return scoreData.GameScores;
    }
}

/// <summary>
/// 1譜面に1つのスコアを保持する
/// </summary>
[Serializable]
public class GameScore
{
    readonly string fumenAddress;
    int score;
    bool isFullCombo;

    public string FumenAddress => fumenAddress;
    public int Score => score;
    public bool IsFullCombo => isFullCombo;

    public GameScore(string fumenAddress, int score, bool isFullCombo)
    {
        this.fumenAddress = fumenAddress;
        this.score = score;
        this.isFullCombo = isFullCombo;
    }

    public void UpdateScore(int score, bool isFullCombo)
    {
        this.score = score;
        this.isFullCombo = isFullCombo;
    }
}