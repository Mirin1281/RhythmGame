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
    public float BGMVolume { get; set; } = 0.8f;

    [JsonProperty("設定: SE音量")]
    public float SEVolume { get; set; } = 0.8f;

    [JsonProperty("設定: ノーツ音量")]
    public float NoteVolume { get; set; } = 0.8f;

    [JsonProperty("設定: スピード")]
    public int Speed { get; set; } = 70;

    [JsonProperty("設定: スピード3D")]
    public int Speed3D { get; set; } = 70;

    [JsonProperty("設定: オフセット")]
    public int Offset { get; set; } = 0;

    [JsonProperty("設定: ミラー譜面")]
    public bool IsMirror { get; set; }
    

    [JsonProperty("難易度")]
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;

    [JsonProperty("選択した曲インデックス")]
    public int SelectedIndex { get; set; } = -1;
}