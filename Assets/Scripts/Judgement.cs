using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;

public enum NoteGrade
{
    None,
    Perfect,
    FastGreat,
    LateGreat,
    FastFar,
    LateFar,
    Miss,
}

public class Judgement : MonoBehaviour
{
    [SerializeField] TMP_Text comboText;
    [SerializeField] TMP_Text deltaText;
    [SerializeField] ParticleManager particleManager;
    [SerializeField] LightParticle[] lights;
    readonly Dictionary<ArcNote, LightParticle> lightDic = new(4);

    int combo;

    const float Range = 2.4f;
    public bool IsNearPosition(Vector2 pos1, Vector2 pos2, float rangeW = Range)
    {
        var distance = Vector2.Distance(pos1, pos2);
        return distance < rangeW;
    }

    public void PlayParticle(NoteGrade grade, Vector2 pos)
    {
        particleManager.PlayParticle(grade, pos);
    }

    public void AddCombo()
    {
        combo++;
        comboText.SetText(combo.ToString());
    }
    public void ResetCombo()
    {
        combo = 0;
        comboText.SetText(combo.ToString());
    }

    public NoteGrade GetGradeAndSetText(float delta)
    {
        var grade = GetGrade(delta);
        deltaText.SetText(Mathf.RoundToInt(delta * 1000f).ToString());
        return grade;
    }

    public static NoteGrade GetGrade(float delta)
    {
        if(Mathf.Abs(delta) < 0.05f)
        {
            return NoteGrade.Perfect;
        }
        else if(Mathf.Abs(delta) < 0.08f)
        {
            if(delta > 0)
            {
                return NoteGrade.LateGreat;
            }
            else
            {
                return NoteGrade.FastGreat;
            }
        }
        else if(Mathf.Abs(delta) < 0.12f)
        {
            if(delta > 0)
            {
                return NoteGrade.LateFar;
            }
            else
            {
                return NoteGrade.FastFar;
            }
        }
        else
        {
            return NoteGrade.Miss;
        }
    }

    LightParticle GetLight(ArcNote arcNote)
    {
        if (lightDic.ContainsKey(arcNote))
        {
            return lightDic[arcNote];
        }
        else
        {
            foreach(var p in lights)
            {
                if(lightDic.ContainsValue(p)) continue;
                lightDic.Add(arcNote, p);
                return p;
            }
        }
        return null;
    }
    public void SetShowLight(ArcNote arcNote, Vector2 pos, bool enabled)
    {
        LightParticle light = GetLight(arcNote);
        if(light != null)
        {
            light.IsActive = enabled;
            if(enabled)
            {
                light.SetPos(pos);
            }
        }
    }
    public void RemoveLink(ArcNote arcNote)
    {
        if(lightDic.ContainsKey(arcNote) == false) return;
        lightDic[arcNote].IsActive = false;
        lightDic.Remove(arcNote);
    }
}
