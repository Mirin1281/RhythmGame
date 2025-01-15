using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using NoteGenerating;

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
    [SerializeField] TMP_Text judgeText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] ParticlePool particlePool;
#if UNITY_EDITOR
    [SerializeField] bool showDebugRange;
    [SerializeField] GameObject debugNoteRangePrefab;
#else
    bool showDebugRange = false;
#endif

    Result result;
    float showScore;
    CancellationTokenSource cts = new();

    public Result Result => result;
    const float Range = 4.6f;
    const float ArcRange = 5.3f;

    public void Init(FumenData fumenData)
    {
        result = new Result(fumenData);
    }
    void Awake()
    {
        comboText.SetText("0");
        judgeText.SetText("");
    }
    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        result = null;
    }


    public bool IsNearPosition(NoteExpect expect, Vector2 inputPos)
    {
        return MyUtility.IsPointInsideRectangle(
            new Rect(expect.Pos, new Vector2(expect.Note.Width * Range, expect.Note.IsVerticalRange ? 30f : Range)),
            inputPos,
            expect.Note.transform.eulerAngles.z);
    }
    public bool IsNearPositionHold(HoldNote hold, Vector2 inputPos)
    {
        return MyUtility.IsPointInsideRectangle(
            new Rect(hold.GetLandingPos(), new Vector2(hold.Width * Range, hold.IsVerticalRange ? 30f : Range)),
            inputPos,
            hold.transform.eulerAngles.z);
    }
    public bool IsNearPositionArc(Vector2 pos1, Vector2 pos2)
    {
        var distance = Vector2.Distance(pos1, pos2);
        return distance < ArcRange / 2f;
    }

    public void PlayParticle(NoteGrade grade, Vector2 pos)
    {
        particlePool.PlayParticle(pos, grade);
    }

    public void SetCombo(NoteGrade grade)
    {
        int beforeScore = result.Score;
        result.SetComboAndScore(grade);
        comboText.SetText("{0}", result.Combo);
        SetScoreTextAsync(beforeScore, result.Score).Forget();


        async UniTaskVoid SetScoreTextAsync(float beforeScore, float toScore)
        {
            float baseTime = Metronome.Instance.CurrentTime;
            float t = 0;
            var easing = new Easing(beforeScore, toScore, 0.3f, EaseType.OutQuad);
            while (t < 0.3f)
            {
                t = Metronome.Instance.CurrentTime - baseTime;
                var s = easing.Ease(t);
                if (showScore < s)
                {
                    showScore = s;
                    scoreText.SetText("{0:00000000}", s);
                }

                await UniTask.Yield(destroyCancellationToken);
            }
        }
    }

    public NoteGrade GetGradeAndSetText(float delta)
    {
        var grade = GetGrade(delta);
        if (grade != NoteGrade.Perfect)
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
        if (Mathf.Abs(delta) < 0.05f)
        {
            return NoteGrade.Perfect;
        }
        else if (Mathf.Abs(delta) < 0.10f)
        {
            if (delta > 0)
            {
                return NoteGrade.LateGreat;
            }
            else
            {
                return NoteGrade.FastGreat;
            }
        }
        else if (Mathf.Abs(delta) < 0.15f)
        {
            if (delta > 0)
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

#if UNITY_EDITOR
    public void DebugShowRange(NoteExpect expect)
    {
        if (showDebugRange == false) return;
        var obj = Instantiate(debugNoteRangePrefab, transform);
        obj.transform.localPosition = expect.Pos;
        obj.transform.localRotation = Quaternion.AngleAxis(expect.Note.transform.eulerAngles.z, Vector3.forward);
        obj.transform.localScale = new Vector3(expect.Note.Width * Range, expect.Note.IsVerticalRange ? 30f : Range);
        UniTask.Void(async () =>
        {
            await MyUtility.WaitSeconds(0.15f, destroyCancellationToken);
            Destroy(obj);
        });
    }
    public void DebugShowRange(Vector2 pos)
    {
        var obj = Instantiate(debugNoteRangePrefab, transform);
        obj.transform.localPosition = pos;
        obj.transform.localScale = new Vector3(2f, 2f);
        UniTask.Void(async () =>
        {
            await MyUtility.WaitSeconds(0.15f, destroyCancellationToken);
            Destroy(obj);
        });
    }
#endif
}
