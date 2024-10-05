using System;
using System.Collections.Generic;
using NoteGenerating;
using UnityEngine;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(
    fileName = "MasterManager",
    menuName = "ScriptableObject/MasterManager",
    order = 4)
]
public class MusicMasterManagerData : ScriptableObject
{
    [SerializeField] MusicMasterData[] masterDatas;
    static readonly string FileName = "ScoreData";
    public MusicMasterData[] MasterDatas => masterDatas;

    IReadOnlyList<FumenData> GetFumenDatas()
    {
        List<FumenData> fumenDatas = new(Mathf.RoundToInt(masterDatas.Length * 2.5f));
        foreach(var data in masterDatas)
        {
            fumenDatas.Add(data.GetFumenData(Difficulty.Normal));
            fumenDatas.Add(data.GetFumenData(Difficulty.Hard));
            var extraFumenData = data.GetFumenData(Difficulty.Extra);
            if(extraFumenData != null)
            {
                fumenDatas.Add(extraFumenData);
            }
        }
        return fumenDatas;
    }

    [ContextMenu("ResetScoreData")]
    void ResetScoreData()
    {
        var scoreData = new ScoreData();
        var fumenDatas = GetFumenDatas();
        for(int i = 0; i < fumenDatas.Count; i++)
        {
            scoreData.GameScores.Add(new GameScore(fumenDatas[i].name, 0, false));
        }
        SaveLoadUtility.SetDataImmediately(scoreData, FileName);
        Debug.Log("データをリセットしました");
    }

    /// <summary>
    /// リザルト時に更新があったら呼び出されます
    /// </summary>
    void SetDataToJson(GameScore gameScore)
    {

    }
}

/// <summary>
/// 1譜面に1つのスコアを保持する
/// </summary>
[Serializable]
public readonly struct GameScore
{
    [SerializeField] readonly string fumenName;
    [SerializeField] readonly int score;
    [SerializeField] readonly bool isFullCombo;

    public readonly string FumenName => fumenName;
    public readonly int Score => score;
    public readonly bool IsFullCombo => isFullCombo;

    public GameScore(string fumenName, int score, bool isFullCombo)
    {
        this.fumenName = fumenName;
        this.score = score;
        this.isFullCombo = isFullCombo;
    }
}

[JsonObject]
public class ScoreData
{
    [JsonProperty("譜面ごとのスコアリスト")]
    public List<GameScore> GameScores { get; set; } = new();
}