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

    /// <summary>
    /// 50-100の範囲を想定
    /// </summary>
    [JsonProperty("スピード")]
    public int Speed { get; set; } = 70;

    /// <summary>
    /// -100-100の範囲を想定
    /// </summary>
    [JsonProperty("オフセット")]
    public int Offset { get; set; } = 0;

    [JsonProperty("ミラー譜面")]
    public bool IsMirror { get; set; } = false;

    [JsonProperty("ダークモード")]
    public bool IsDark { get; set; } = false;

    [JsonProperty("オートプレイ")]
    public bool IsAutoPlay { get; set; } = false;

    [JsonProperty("コンボ数を上に表示")]
    public bool IsComboAbove { get; set; } = false;

    [JsonProperty("精度を表示")]
    public bool IsShowAccuracy { get; set; } = true;
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