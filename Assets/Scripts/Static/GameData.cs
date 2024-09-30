using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class GameData
{
    [JsonProperty("設定: BGM音量")]
    public float BgmVolume { get; set; } = 0.3f;

    [JsonProperty("設定: SE音量")]
    public float SeVolume { get; set; } = 0.3f;

    /*[JsonProperty("設定: ランダム選曲")]
    public bool IsRandomBGM { get; set; } = false;

    [JsonProperty("設定: ガイド表示")]
    public bool IsGuideShow { get; set; } = true;

    [JsonProperty("設定: クラップ音")]
    public bool IsPlayClapSound { get; set; } = true;

    [JsonProperty("起動回数")]
    public int BootCount { get; set; }

    [JsonProperty("最後に起動した時間")]
    public string LastBootTime { get; set; }

    [JsonProperty("最後に終了した時間")]
    public string LastQuitTime { get; set; }

    [JsonProperty("ステージのクリア状況")]
    public Dictionary<string, bool> StageClearDictionary { get; set; } = new(30);

    [JsonProperty("フラグ")]
    public Dictionary<string, bool> FlagDictionary { get; set; } = new();

    [JsonProperty("ステージ8-4のゴール地点")]
    public int Stage8_4GoalIndex { get; set; } = -1;

    [JsonProperty("ステージ8-4の移動記録")]
    public List<Direction> Stage8_4MoveHistory { get; set; }

    [JsonProperty("立ち絵クリック回数")]
    public int TachieClickCount { get; set; }

    [JsonProperty("総ステップ数")]
    public int TotalStepCount { get; set; }*/
}