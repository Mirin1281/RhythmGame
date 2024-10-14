using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[CreateAssetMenu(
    fileName = "MasterManager",
    menuName = "ScriptableObject/MasterManager",
    order = 4)
]
public class MusicMasterManagerData : ScriptableObject
{
    [SerializeField] MusicSelectData[] selectDatas;
    public MusicSelectData[] SelectDatas => selectDatas;

    [ContextMenu("ResetScoreData")]
    ScoreData ResetScoreData()
    {
        var scoreData = new ScoreData();
        for(int i = 0; i < selectDatas.Length; i++)
        {
            var d = selectDatas[i];
            if(!string.IsNullOrEmpty(d.NormalFumenAddress))
            {
                scoreData.GameScores.Add(new GameScore(d.NormalFumenAddress, 0, false));
            }
            if(!string.IsNullOrEmpty(d.HardFumenAddress))
            {
                scoreData.GameScores.Add(new GameScore(d.HardFumenAddress, 0, false));
            }
            if(!string.IsNullOrEmpty(d.ExtraFumenAddress))
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
    /// 返り値は元のスコアです
    /// </summary>
    public async UniTask<int> SetScoreJsonAsync(GameScore gameScore)
    {
        var scoreData = await SaveLoadUtility.GetData<ScoreData>(ConstContainer.ScoreDataName);
        scoreData ??= ResetScoreData();

        int index = -1;
        GameScore s = null;
        for(int i = 0; i < scoreData.GameScores.Count; i++)
        {
            if(scoreData.GameScores[i].FumenName == gameScore.FumenName)
            {
                s = scoreData.GameScores[i];
                index = i;
            }
        }

        int beforeScore = s.Score;
        int score = s.Score;
        if(s.Score < gameScore.Score)
        {
            score = gameScore.Score;
        }

        bool isFullCombo = s.IsFullCombo;
        if(isFullCombo == false && gameScore.IsFullCombo)
        {
            isFullCombo = gameScore.IsFullCombo;
        }

        scoreData.GameScores[index] = new GameScore(s.FumenName, score, isFullCombo);
        await SaveLoadUtility.SetData(scoreData, ConstContainer.ScoreDataName);
        Debug.Log("データが保存されました");
        return beforeScore;
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
    [SerializeField] readonly string fumenName;
    [SerializeField] readonly int score;
    [SerializeField] readonly bool isFullCombo;

    public string FumenName => fumenName;
    public int Score => score;
    public bool IsFullCombo => isFullCombo;

    public GameScore(string fumenName, int score, bool isFullCombo)
    {
        this.fumenName = fumenName;
        this.score = score;
        this.isFullCombo = isFullCombo;
    }
}