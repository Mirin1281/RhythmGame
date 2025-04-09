using NoteCreating;
using UnityEngine;

public class Result
{
    int perfect;
    int fastGreat;
    int lateGreat;
    int fastFar;
    int lateFar;
    int miss;

    int combo;
    int maxCombo;
    double score;
    int accuracyCount; // 叩けた精度があるノーツの数
    float totalDelta; // 精度の合計
    readonly int noteCount;
    readonly MusicSelectData musicData;

    static readonly int MaxScore = 10000000;

    public int Perfect => perfect;
    public int FastGreat => fastGreat;
    public int LateGreat => lateGreat;
    public int FastFar => fastFar;
    public int LateFar => lateFar;
    public int Miss => miss;

    public int Combo => combo;
    public int MaxCombo => maxCombo;
    public int Score => Mathf.RoundToInt((float)score);
    public void AddTotalDelta(float delta)
    {
        accuracyCount++;
        totalDelta += delta;
    }
    public float AverageDelta => totalDelta / accuracyCount;
    public MusicSelectData MusicData => musicData;
    public bool IsFullCombo => miss == 0;

    public Result(FumenData fumenData)
    {
        noteCount = fumenData.NoteCount;
        this.musicData = fumenData.MusicSelectData;
    }

    public void SetComboAndScore(NoteGrade grade)
    {
        switch (grade)
        {
            case NoteGrade.Perfect:
                perfect++;
                break;
            case NoteGrade.FastGreat:
                fastGreat++;
                break;
            case NoteGrade.LateGreat:
                lateGreat++;
                break;
            case NoteGrade.FastFar:
                fastFar++;
                break;
            case NoteGrade.LateFar:
                lateFar++;
                break;
            case NoteGrade.Miss:
                combo = 0;
                miss++;
                return; // Missの場合ここでreturn
            default:
                Debug.LogError("Invalid: " + grade);
                break;
        }

        combo++;
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }

        double rate = grade switch
        {
            NoteGrade.Perfect => 1d,
            NoteGrade.FastGreat or NoteGrade.LateGreat => 0.5d,
            NoteGrade.FastFar or NoteGrade.LateFar => 0.3d,
            NoteGrade.Miss => 0,
            _ => 0,
        };
        double baseScore = (double)MaxScore / noteCount;
        score += baseScore * rate;
    }
}