using NoteGenerating;
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
    readonly FumenData fumenData;
    
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
    public FumenData FumenData => fumenData;
    public bool IsFullCombo => miss == 0;

    public Result(FumenData fumenData)
    {
        this.fumenData = fumenData;
    }

    public void SetComboAndScore(NoteGrade grade)
    {
        if(grade == NoteGrade.Miss)
        {
            combo = 0;
            miss++;
            return;
        }

        combo++;
        if(combo > maxCombo)
        {
            maxCombo = combo;
        }
        
        if(grade == NoteGrade.Perfect) {
            perfect++;
        } else if(grade == NoteGrade.FastGreat) {
            fastGreat++;
        } else if(grade == NoteGrade.LateGreat) {
            lateGreat++;
        } else if(grade == NoteGrade.FastFar) {
            fastFar++;
        } else if(grade == NoteGrade.LateFar) {
            lateFar++;
        } else {
            throw new System.Exception();
        }

        double rate = grade switch
        {
            NoteGrade.Perfect => 1d,
            NoteGrade.FastGreat or NoteGrade.LateGreat => 0.5d,
            NoteGrade.FastFar or NoteGrade.LateFar => 0.3d,
            NoteGrade.Miss => 0,
            _ => throw new System.Exception()
        };
        double baseScore = (double)MaxScore / fumenData.NoteCount;
        score += baseScore * rate;
    }
}