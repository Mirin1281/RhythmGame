using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NoteGenerating;
using UnityEngine;

[JsonObject]
public abstract class JsonObject { }

public class ScoreData : JsonObject
{
    [JsonProperty("譜面ごとのスコアリスト")]
    public List<GameScore> GameScores { get; set; } = new();
}


public class GameData : JsonObject
{
    [JsonProperty("設定: BGM音量")]
    public float BgmVolume { get; set; } = 0.8f;

    [JsonProperty("設定: SE音量")]
    public float SeVolume { get; set; } = 0.8f;

    [JsonProperty("設定: スピード")]
    public float Speed { get; set; } = 14f;

    [JsonProperty("設定: オフセット")]
    public float Offset { get; set; } = 0f;

    [JsonProperty("難易度")]
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;

    [JsonProperty("選択した曲インデックス")]
    public int SelectedIndex { get; set; } = -1;
}