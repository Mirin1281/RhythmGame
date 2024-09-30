using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject]
public class GameData
{
    [JsonProperty("�ݒ�: BGM����")]
    public float BgmVolume { get; set; } = 0.3f;

    [JsonProperty("�ݒ�: SE����")]
    public float SeVolume { get; set; } = 0.3f;

    /*[JsonProperty("�ݒ�: �����_���I��")]
    public bool IsRandomBGM { get; set; } = false;

    [JsonProperty("�ݒ�: �K�C�h�\��")]
    public bool IsGuideShow { get; set; } = true;

    [JsonProperty("�ݒ�: �N���b�v��")]
    public bool IsPlayClapSound { get; set; } = true;

    [JsonProperty("�N����")]
    public int BootCount { get; set; }

    [JsonProperty("�Ō�ɋN����������")]
    public string LastBootTime { get; set; }

    [JsonProperty("�Ō�ɏI����������")]
    public string LastQuitTime { get; set; }

    [JsonProperty("�X�e�[�W�̃N���A��")]
    public Dictionary<string, bool> StageClearDictionary { get; set; } = new(30);

    [JsonProperty("�t���O")]
    public Dictionary<string, bool> FlagDictionary { get; set; } = new();

    [JsonProperty("�X�e�[�W8-4�̃S�[���n�_")]
    public int Stage8_4GoalIndex { get; set; } = -1;

    [JsonProperty("�X�e�[�W8-4�̈ړ��L�^")]
    public List<Direction> Stage8_4MoveHistory { get; set; }

    [JsonProperty("�����G�N���b�N��")]
    public int TachieClickCount { get; set; }

    [JsonProperty("���X�e�b�v��")]
    public int TotalStepCount { get; set; }*/
}