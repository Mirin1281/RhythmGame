using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public abstract class JsonObject { }

public class GameData : JsonObject
{
    [JsonProperty("設定")]
    public GameSetting Setting { get; set; }

    [JsonProperty("その他の状態")]
    public GameStatus Status { get; set; }
}

[Serializable]
public class GameSetting
{
    [JsonProperty("BGM音量")]
    public float BGMVolume { get; set; } = 0.8f;

    [JsonProperty("SE音量")]
    public float SEVolume { get; set; } = 0.8f;

    [JsonProperty("ノーツ音量")]
    public float NoteSEVolume { get; set; } = 0.8f;

    [JsonProperty("ノーツ音量のミュート")]
    public bool IsNoteMute { get; set; } = false;

    [JsonProperty("スピード")]
    public int Speed { get; set; } = 70;

    [JsonProperty("オフセット")]
    public int Offset { get; set; } = 0;

    [JsonProperty("ミラー譜面")]
    public bool IsMirror { get; set; } = false;

    [JsonProperty("厳しめ判定")]
    public bool IsStrictJudge { get; set; } = false;

    [JsonProperty("オートプレイ")]
    public bool isAutoPlay { get; set; } = false;
}

[Serializable]
public class GameStatus
{
    [JsonProperty("難易度")]
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;

    [JsonProperty("選択した曲インデックス")]
    public int SelectedIndex { get; set; } = -1;
}


public class ScoreData : JsonObject
{
    [JsonProperty("譜面ごとのスコアリスト")]
    public List<GameScore> GameScores { get; set; } = new();
}