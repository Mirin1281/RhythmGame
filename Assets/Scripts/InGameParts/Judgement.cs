using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

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
    [SerializeField] InGameManager inGameManager;
    [SerializeField] TMP_Text comboText;
    [SerializeField] TMP_Text deltaText;
    [SerializeField] TMP_Text judgeText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] ParticleManager particleManager;
#if UNITY_EDITOR
    [SerializeField] bool showDebugRange;
#else
    bool showDebugRange = false;
#endif
    [SerializeField] GameObject debugNoteRangePrefab;
    [SerializeField] LightParticle[] lights;
    
    Dictionary<ArcNote, LightParticle> lightDic = new(4);
    Result result;
    public Result Result => result;

    void Awake()
    {
        comboText.SetText("0");
        judgeText.SetText("");
        lightDic = new(4);
        UniTask.Void(async () => 
        {
            await UniTask.WaitUntil(() => inGameManager.IsLoaded, cancellationToken: destroyCancellationToken);
            result = new Result(inGameManager.FumenData);
        });
    }

    const float Range = 4.6f;
    public bool IsNearPositionArc(Vector2 pos1, Vector2 pos2, float rangeW = Range)
    {
        var distance = Vector2.Distance(pos1, pos2);
        return distance < rangeW / 2f;
    }
    public bool IsNearPositionHold(HoldNote hold, Vector2 inputPos)
    {
        return MyUtility.IsPointInsideRectangle(
            new Rect(hold.GetLandingPos(), new Vector2(hold.Width * Range, Range)),
            inputPos,
            hold.transform.eulerAngles.z);
    }
    public bool IsNearPosition(NoteExpect expect, Vector2 inputPos)
    {
        return MyUtility.IsPointInsideRectangle(
            new Rect(expect.Pos, new Vector2(expect.Note.Width * Range, Range)),
            inputPos,
            expect.Note.transform.eulerAngles.z);
    }

    public void PlayParticle(NoteGrade grade, Vector2 pos)
    {
        particleManager.PlayParticle(grade, pos);
    }

    public async UniTask DebugShowRange(NoteExpect expect)
    {
        if(showDebugRange == false) return;
        var obj = Instantiate(debugNoteRangePrefab, transform);
        obj.transform.localPosition = expect.Pos;
        obj.transform.localRotation = Quaternion.AngleAxis(expect.Note.transform.eulerAngles.z, Vector3.forward);
        obj.transform.localScale = new Vector3(expect.Note.Width * 4.6f, 4.6f);
        await MyUtility.WaitSeconds(0.15f, destroyCancellationToken);
        Destroy(obj);
    }
    public async UniTask DebugShowRange(Vector2 pos)
    {
        var obj = Instantiate(debugNoteRangePrefab, transform);
        obj.transform.localPosition = pos;
        obj.transform.localScale = new Vector3(2f, 2f);
        await MyUtility.WaitSeconds(0.15f, destroyCancellationToken);
        Destroy(obj);
    }

    public void SetCombo(NoteGrade grade)
    {
        result.SetCombo(grade);
        comboText.SetText("{0}", result.Combo);
        scoreText.SetText("{0:00000000}", result.Score);
    }

    CancellationTokenSource cts = new();
    public NoteGrade GetGradeAndSetText(float delta)
    {
        var grade = GetGrade(delta);
        if(grade != NoteGrade.Perfect)
            SetJudgeText(grade).Forget();
        deltaText.SetText("{0}", Mathf.RoundToInt(delta * 1000f));
        return grade;


        async UniTask SetJudgeText(NoteGrade grade)
        {
            cts.Cancel();
            cts = new();
            CancellationToken token = cts.Token;

            judgeText.SetText(grade.ToString());
            await MyUtility.WaitSeconds(1f, token);
            judgeText.SetText(string.Empty);
        }
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
