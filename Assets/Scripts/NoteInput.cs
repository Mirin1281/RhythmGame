using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HoldState = HoldNote.InputState;
using ArcJudgeState = ArcJudge.InputState;

#region Note Parameters

public enum NoteType
{
    _None, // Normalと間違えやすいため
    Normal,
    Slide,
    Hold,
    Flick,
    Arc,
}

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

public class NoteExpect
{
    public readonly NoteBase Note;
    public readonly Vector2 Pos;
    public readonly float Time;
    public readonly float HoldEndTime;

    public NoteExpect(NoteBase note, Vector2 pos, float time, float holdTime = 0)
    {
        Note = note;
        Pos = pos;
        Time = time;
        HoldEndTime = holdTime;
    }
}

#endregion

public class NoteInput : MonoBehaviour
{
    [SerializeField] Metronome metronome;
    [SerializeField] InputManager inputManager;
    [SerializeField] TMP_Text comboText;
    [SerializeField] TMP_Text deltaText;
    [SerializeField] ParticleManager particleManager;
    [SerializeField] ParticleSystem particle;
    int listcount;
    readonly List<NoteExpect> allExpects = new(50);
    readonly List<(HoldNote hold, float endTime)> holds = new(4);
    readonly List<ArcNote> arcs = new(4);
#if UNITY_EDITOR
    [SerializeField] bool isAuto;
#else
    bool isAuto = false;
#endif
    int combo;

    void Start()
    {
        ResetCombo();
        if(isAuto) return;
        inputManager.OnInput += OnInput;
        inputManager.OnHold += OnHold;
        inputManager.OnFlick += OnFlick;
    }
    void OnDestroy()
    {
        inputManager.OnInput -= OnInput;
        inputManager.OnHold -= OnHold;
        inputManager.OnFlick -= OnFlick;
    }

    public void AddExpect(NoteExpect expect)
    {
        allExpects.Add(expect);
    }
    void RemoveExpect(NoteExpect expect, bool isInactive = true)
    {
        expect.Note.gameObject.SetActive(!isInactive);
        allExpects.Remove(expect);
    }

    HoldNote AddHold(NoteExpect expect)
    {
        var hold = expect.Note as HoldNote;
        hold.State = HoldState.Holding;
        holds.Add((hold, expect.HoldEndTime));
        return hold;
    }

    public void AddArc(ArcNote arcNote)
    {
        arcs.Add(arcNote);
    }

    void Update()
    {
        for(int i = 0; i < allExpects.Count; i++)
        {
            if(isAuto)
            {
                AutoGet(allExpects[i]);
            }
            else
            {
                Miss(allExpects[i]);
            }
        }
        var poses = inputManager.GetScreenPositions();
        CheckHold(poses);
        CheckArc(poses);

        
        void AutoGet(NoteExpect expect)
        {
            if(metronome.CurrentTime > expect.Time - 0.02f)
            {
                if(expect.Note.Type == NoteType.Hold)
                {
                    particleManager.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    HoldNote hold = AddHold(expect);
                    hold.Grade = NoteGrade.Perfect;
                    RemoveExpect(expect, false);
                }
                else if(expect.Note.Type == NoteType.Arc)
                {
                    AddArc(expect.Note as ArcNote);
                    RemoveExpect(expect, false);
                }
                else
                {
                    particleManager.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    RemoveExpect(expect);
                    AddCombo();
                }
            }
        }

        void Miss(NoteExpect expect)
        {
            // 遅れたらノーツを除去
            if(metronome.CurrentTime > expect.Time + 0.18f)
            {
                if(expect.Note.Type == NoteType.Hold)
                {
                    AddHold(expect);
                    RemoveExpect(expect, false);
                    ResetCombo();
                }
                else
                {
                    RemoveExpect(expect);
                    ResetCombo();
                }
            }
        }

        void CheckHold(Vector2[] inputPoses)
        {
            if(holds.Count == 0) return;
            for(int i = 0; i < holds.Count; i++)
            {
                var hold = holds[i].hold;
                if(hold.State is HoldState.None or HoldState.Idle)
                {
                    throw new System.Exception();
                }
                else if(hold.State is HoldState.Holding)
                {
                    bool isInput = false;
                    for(int k = 0; k < inputPoses.Length; k++)
                    {
                        if(IsNearPosition(inputPoses[k], hold.GetLandingPos()))
                        {
                            isInput = true;
                            break;
                        }
                    }
                                
                    if(isInput || isAuto)
                    {
                        // ギリギリまで取らなくても判定されるように
                        if(metronome.CurrentTime > holds[i].endTime - 0.16f)
                        {
                            AddCombo();
                            hold.State = HoldState.Got;
                        }
                    }
                    else
                    {
                        hold.State = HoldState.Missed;
                        ResetCombo();
                    }
                }
                else if(hold.State == HoldState.Missed)
                {
                    if(metronome.CurrentTime > holds[i].endTime)
                    {
                        hold.gameObject.SetActive(false);
                        holds.RemoveAt(i);
                    }
                }
                else if(hold.State == HoldState.Got)
                {
                    // ちょっと早めに表示
                    if(metronome.CurrentTime > holds[i].endTime - 0.02f)
                    {
                        hold.gameObject.SetActive(false);
                        holds.RemoveAt(i);
                        particleManager.PlayParticle(hold.Grade, hold.GetLandingPos());
                    }
                }
            }
        }

        void CheckArc(Vector2[] inputPoses)
        {
            if(arcs.Count == 0) return;
            for(int i = 0; i < arcs.Count; i++)
            {
                var arc = arcs[i];
                var arcPos = arc.GetPos();
                if(arcPos.z < 0) return; // まだ到達していない
                if(arcPos.z > arc.LastZ + 1) // アークが完全に通り過ぎた
                {
                    arcs.RemoveAt(i);
                    arc.SetActive(false);
                    particle.gameObject.SetActive(false);
                    return;
                }

                bool isHold = false;
                var currentPos = arc.GetAnyPointOnZPlane(0);
                foreach(var inputPos in inputPoses)
                {
                    if(IsNearPosition(inputPos, currentPos, 1f, 1f))
                    {
                        particle.transform.localPosition = new Vector2(currentPos.x, currentPos.y);
                        isHold = true;
                        break;
                    }
                }
                arc.SetColor(isHold);
                particle.gameObject.SetActive(isHold);

                var judge = arc.GetCurrentJudge();
                if (judge == null) return; // 最後の判定を終えた
                if (judge.EndPos.z < arcPos.z)
                {
                    judge.State = ArcJudgeState.Miss;
                    arc.JudgeIndex++;
                    ResetCombo();
                }

                if ((judge.StartPos.z < arcPos.z && arcPos.z < judge.EndPos.z) == false) return; // 判定の範囲外

                if(judge.State is ArcJudgeState.None)
                {
                    throw new System.Exception();
                }
                else if(judge.State is ArcJudgeState.Idle)
                {
                    if(isHold || isAuto)
                    {
                        judge.State = ArcJudgeState.Got;
                        particleManager.PlayParticle(NoteGrade.Perfect, currentPos);
                        arc.JudgeIndex++;
                        AddCombo();
                    }
                }
            }
        }
    }

    void OnInput(Vector2 pos)
    {
        (NoteExpect expect, float delta) = FetchNearestNote(pos, metronome.CurrentTime, NoteType.Normal, NoteType.Hold);
        if(expect == null) return;

        NoteGrade grade = GetGrade(delta);
        deltaText.SetText(Mathf.RoundToInt(delta * 1000f).ToString());
        if(grade == NoteGrade.Miss)
        {
            ResetCombo();
            RemoveExpect(expect);
            return;
        }
        if(expect.Note.Type == NoteType.Hold)
        {
            var hold = AddHold(expect);
            hold.Grade = grade;
            RemoveExpect(expect, false);
            AddCombo();
            particleManager.PlayParticle(grade, expect.Pos);
            return;
        }
        RemoveExpect(expect);
        AddCombo();
        particleManager.PlayParticle(grade, expect.Pos);
    }

    void OnHold(Vector2[] poses)
    {
        foreach(var pos in poses)
        {
            List<(NoteExpect, float)> expects = FetchSomeNotes(pos, metronome.CurrentTime, NoteType.Slide);
            if(expects == null) return;

            foreach(var (expect, delta) in expects)
            {

                RemoveExpect(expect, false);
                UniTask.Void(async () => 
                {
                    if(delta < 0)
                    {
                        await MyStatic.WaitSeconds(-delta);
                    }
                    
                    particleManager.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    expect.Note.gameObject.SetActive(false);
                    AddCombo();
                });
            }
        }
    }

    void OnFlick(Vector2 pos)
    {
        List<(NoteExpect, float)> expects = FetchSomeNotes(pos, metronome.CurrentTime, NoteType.Flick);
        if(expects == null) return;

        foreach(var (expect, delta) in expects)
        {
            RemoveExpect(expect, false);
            UniTask.Void(async () => 
            {
                if(delta < 0)
                {
                    await MyStatic.WaitSeconds(-delta);
                }
                
                particleManager.PlayParticle(NoteGrade.Perfect, expect.Pos);
                expect.Note.gameObject.SetActive(false);
                AddCombo();
            });
        }
    }

    (NoteExpect, float) FetchNearestNote(Vector2 inputPos, float inputTime, params NoteType[] fetchTypes)
    {
        NoteExpect fetchedExpect = null;
        float timeSample = Mathf.Infinity;
        foreach(var expect in allExpects)
        {
            // inputTimeSampleと落ちてくる時間を比較、近いものを選定
            float delta = inputTime - expect.Time;
            if (Mathf.Abs(delta) > 0.1f) continue;

            // 指定した種類を選定
            bool isMatch = false;
            foreach(var type in fetchTypes)
            {
                if(expect.Note.Type == type)
                {
                    isMatch = true;
                    break;
                }
            }
            if(isMatch == false) continue;

            // inputPosに近いノーツを選定
            if(IsNearPosition(inputPos, expect.Pos) == false) continue;

            // より早く落ちてくるノーツを判定(複数あればよりinputPosに近いノーツを返す)
            if(fetchedExpect == null)
            {
                fetchedExpect = expect;
            }
            float t = expect.Time;
            if(t <= timeSample
            && Vector2.SqrMagnitude(inputPos - expect.Pos) < Vector2.SqrMagnitude(inputPos - fetchedExpect.Pos))
            {
                timeSample = t;
                fetchedExpect = expect;
            }
        }
        if(fetchedExpect == null) return default;

        return (fetchedExpect, inputTime - fetchedExpect.Time);
    }

    List<(NoteExpect, float)> FetchSomeNotes(Vector2 inputPos, float inputTime, params NoteType[] fetchTypes)
    {
        List<(NoteExpect, float)> fetchedExpects = new(4);
        foreach(var expect in allExpects)
        {
            float delta = inputTime - expect.Time;
            if(Mathf.Abs(delta) > 0.25f) continue;

            bool isMatch = false;
            foreach(var type in fetchTypes)
            {
                if(expect.Note.Type == type)
                {
                    isMatch = true;
                    break;
                }
            }
            if(isMatch == false) continue;

            if(IsNearPosition(inputPos, expect.Pos) == false) continue;
            fetchedExpects.Add((expect, delta));
        }
        if(fetchedExpects.Count == 0) return default;
        return fetchedExpects;
    }

    void AddCombo()
    {
        combo++;
        comboText.SetText(combo.ToString());
    }
    void ResetCombo()
    {
        combo = 0;
        comboText.SetText(combo.ToString());
    }

    const float rangeWidth = 2.1f;
    const float rangeHeight = 2.1f;
    bool IsNearPosition(Vector2 pos1, Vector2 pos2, float rangeW = rangeWidth, float rangeH = rangeHeight)
    {
        var deltaPos = pos1 - pos2;
        return Mathf.Abs(deltaPos.x) < rangeWidth && Mathf.Abs(deltaPos.y) < rangeHeight;
    }

    static NoteGrade GetGrade(float delta)
    {
        if(Mathf.Abs(delta) < 0.03f)
        {
            return NoteGrade.Perfect;
        }
        else if(Mathf.Abs(delta) < 0.06f)
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
}
