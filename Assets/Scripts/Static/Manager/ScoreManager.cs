using System.Collections.Generic;
using NoteGenerating;

/// <summary>
/// 譜面データをキーとしてスコアを保存する静的クラスです
/// </summary>
public static class ScoreManager
{
    static Dictionary<string, int> ScoreDictionary = new(30);

    public static void SetScore(FumenData fumenData, int score)
    {
        ScoreDictionary[fumenData.name] = score;
    }

    public static bool TryGetScore(FumenData fumenData, out int score)
    {
        if(ScoreDictionary.ContainsKey(fumenData.name) == false)
        {
            score = -1;
            return false;
        }
        else
        {
            score = ScoreDictionary[fumenData.name];
            return true;
        }
    }

    public static IReadOnlyDictionary<string, int> GetScoreDictionary() => ScoreDictionary;

    public static void SetScoreDictionary(Dictionary<string, int> dic)
    {
        ScoreDictionary = dic;
    }


    /*static Dictionary<string, bool> FlagDictionary = new();

    /// <summary>
    /// ステージクリアのフラグはSetStageClearFlagを使って
    /// </summary>
    /// <param name="flagName"></param>
    /// <param name="isOn"></param>
    public static void SetFlag(AchievementData achievementData, bool isOn = true)
    {
        FlagDictionary[achievementData.AchievementName] = isOn;
    }

    public static bool GetFlag(AchievementData achievementData)
    {
        if (FlagDictionary.ContainsKey(achievementData.AchievementName) == false) return false;
        return FlagDictionary[achievementData.AchievementName];
    }

    public static Dictionary<string, bool> GetFlagDictionary() => FlagDictionary;

    public static void SetFlagDictionary(Dictionary<string, bool> dic)
    {
        FlagDictionary = dic;
    }*/
}