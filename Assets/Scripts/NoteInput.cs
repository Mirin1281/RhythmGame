using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HoldState = HoldNote.InputState;
using ArcJudgeState = ArcJudge.InputState;
using Input = InputManager.Input;
using System.Linq;

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

public class NoteInput : MonoBehaviour
{
    [SerializeField] Metronome metronome;
    [SerializeField] InputManager inputManager;
    [SerializeField] Judgement judge;
    [SerializeField] NotePoolManager notePoolManager;
    [SerializeField] bool isAuto;

    readonly List<NoteExpect> allExpects = new(63);
    readonly List<HoldNote> holds = new(4);
    readonly List<ArcNote> arcs = new(4);
    readonly List<(NoteExpect, float)> fetchedExpects = new(8);

    static readonly float defaultRange = 2.4f;
    static readonly float flickRange = 3f;
    static readonly float arcRange = 2f;

    static readonly float defaultTolerance = 0.1f;
    static readonly float wideTolerance = 0.25f;

    static readonly float arcDuplicateSqrDistance = 8f;

    void Start()
    {
#if PLATFORM_ANDROID
    bool isAuto = false;
#endif
        judge.ResetCombo();
        if(isAuto) return;
        inputManager.OnInput += OnInput;
        inputManager.OnHold += OnHold;
        inputManager.OnFlick += OnFlick;
        inputManager.OnUp += OnUp;


        void OnUp(int fingerIndex)
        {
            UniTask.Void(async () => 
            {
                await UniTask.Yield(destroyCancellationToken);
                await UniTask.Yield(destroyCancellationToken);
                foreach(var arc in arcs)
                {
                    if(arc.FingerIndex == fingerIndex && arc.IsInvalid == false)
                    {
                        arc.InvalidArcJudgeAsync().Forget();
                    }
                }
            });
        }
    }

    public void AddExpect(NoteExpect expect, bool isCheckSimultaneous = true)
    {
        if(isCheckSimultaneous)
        {
            // 同時刻に着地するノーツがあった場合は同時押しの見た目を適用する
            foreach(var e in allExpects)
            {
                if(Mathf.Approximately(expect.Time, e.Time))
                {
                    notePoolManager.SetSimultaneousSprite(expect.Note as NoteBase_2D);
                    notePoolManager.SetSimultaneousSprite(e.Note as NoteBase_2D);
                }
            }
        }
       
        allExpects.Add(expect);
    }

    void RemoveExpect(NoteExpect expect, bool isInactive = true)
    {
        expect.Note.SetActive(!isInactive);
        allExpects.Remove(expect);
    }

    HoldNote AddHold(NoteExpect expect)
    {
        var hold = expect.Note as HoldNote;
        hold.State = HoldState.Holding;
        hold.EndTime = expect.HoldEndTime;
        holds.Add(hold);
        return hold;
    }

    public void AddArc(ArcNote arcNote)
    {
        arcs.Add(arcNote);
    }

    void Update()
    {
        if(isAuto)
        {
            ProcessHold(null);
            ProcessArc(null);
        }

        for(int i = 0; i < allExpects.Count; i++)
        {
            var expect = allExpects[i];
            /*if(isAuto && metronome.CurrentTime > expect.Time - 0.02f)
            {
                // オート
                if(expect.Note.Type == NoteType.Hold)
                {
                    var delta = metronome.CurrentTime - expect.Time;
                    NoteGrade grade = judge.GetGradeAndSetText(delta);
                    judge.PlayParticle(grade, expect.Pos);
                    HoldNote hold = AddHold(expect);
                    hold.Grade = grade;
                    RemoveExpect(expect, false);
                    judge.AddCombo();
                }
                else if(expect.Note.Type == NoteType.Normal)
                {
                    var delta = metronome.CurrentTime - expect.Time;
                    NoteGrade grade = judge.GetGradeAndSetText(delta);
                    judge.PlayParticle(grade, expect.Pos);
                    RemoveExpect(expect);
                    judge.AddCombo();
                }
                else
                {
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    RemoveExpect(expect);
                    judge.AddCombo();
                }
            }*/
            if(isAuto && metronome.CurrentTime > expect.Time)
            {
                // オート
                if(expect.Note.Type == NoteType.Hold)
                {
                    HoldNote hold = AddHold(expect);
                    hold.Grade = NoteGrade.Perfect;
                    RemoveExpect(expect, false);
                }
                else
                {
                    RemoveExpect(expect);
                }
                judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                judge.AddCombo();
            }
            else if(metronome.CurrentTime > expect.Time + 0.18f)
            {
                // 遅れたらノーツを除去
                if(expect.Note.Type == NoteType.Hold)
                {
                    AddHold(expect);
                    RemoveExpect(expect, false);
                }
                else
                {
                    RemoveExpect(expect);
                }
                judge.ResetCombo();
            }
        }
    }

    void OnInput(Vector2 pos)
    {
        (NoteExpect expect, float delta) = FetchNearestNote(
            pos, metronome.CurrentTime, defaultRange, NoteType.Normal, NoteType.Hold, NoteType.Sky);
        if(expect == null) return;

        NoteGrade grade = judge.GetGradeAndSetText(delta);
        if(grade == NoteGrade.Miss)
        {
            judge.ResetCombo();
            RemoveExpect(expect);
            return;
        }

        judge.AddCombo();
        judge.PlayParticle(grade, expect.Pos);

        if(expect.Note.Type == NoteType.Hold)
        {
            var hold = AddHold(expect);
            hold.Grade = grade;
            RemoveExpect(expect, false);
        }
        else
        {
            RemoveExpect(expect);
        }
    }

    void OnHold(List<Input> inputs)
    {
        if(isAuto == false)
        {
            ProcessHold(inputs);
            ProcessArc(inputs);
        }
        
        foreach(var input in inputs)
        {
            List<(NoteExpect, float)> expects = FetchSomeNotes(input.pos, metronome.CurrentTime, defaultRange, wideTolerance);
            if(expects == null) continue;

            foreach(var (expect, delta) in expects)
            {
                if(expect.Note.Type != NoteType.Slide) continue;
                RemoveExpect(expect, false);
                UniTask.Void(async () => 
                {
                    if(delta < 0)
                    {
                        await MyUtility.WaitSeconds(-delta, destroyCancellationToken);
                    }
                    
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    expect.Note.SetActive(false);
                    judge.AddCombo();
                });
            }
        }
    }
    
    void OnFlick(Vector2 pos)
    {
        List<(NoteExpect, float)> expects = FetchSomeNotes(pos, metronome.CurrentTime, flickRange, wideTolerance);
        if(expects == null) return;

        foreach(var (expect, delta) in expects)
        {
            if(expect.Note.Type != NoteType.Flick) continue;
            RemoveExpect(expect, false);
            UniTask.Void(async () => 
            {
                if(delta < 0)
                {
                    await MyUtility.WaitSeconds(-delta, destroyCancellationToken);
                }
                
                judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                expect.Note.SetActive(false);
                judge.AddCombo();
            });
        }
    }

    (NoteExpect, float) FetchNearestNote(Vector2 inputPos, float inputTime, float range, params NoteType[] fetchTypes)
    {
        /*NoteExpect fetchedExpect = null;
        foreach(var expect in allExpects)
        {
            // inputTimeと落ちてくる時間を比較、近いものを選定
            float delta = inputTime - expect.Time;
            if (Mathf.Abs(delta) > normalTolerance) continue;

            // inputPosに近いノーツを選定
            if(judge.IsNearPosition(inputPos, expect.Pos, range) == false) continue;

            // より早く落ちてくるノーツを判定(複数あればよりinputPosに近いノーツを返す)
            fetchedExpect ??= expect;
            if (expect.Time <= fetchedExpect.Time
             && Vector2.SqrMagnitude(inputPos - expect.Pos) < Vector2.SqrMagnitude(inputPos - fetchedExpect.Pos))
            {
                fetchedExpect = expect;
            }
        }

        if(fetchedExpect == null) return default;

        if(fetchedExpect.Note.Type != NoteType.Normal)
        {
            NoteExpect secondFetchedExpect = null;
            foreach(var expect in allExpects)
            {
                float delta = inputTime - expect.Time;
                if (Mathf.Abs(delta) > normalTolerance) continue;
                if(judge.IsNearPosition(inputPos, expect.Pos, range) == false) continue;
                if(expect == fetchedExpect) continue;

                secondFetchedExpect ??= expect;
                if (expect.Time <= secondFetchedExpect.Time
                && Vector2.SqrMagnitude(inputPos - expect.Pos) < Vector2.SqrMagnitude(inputPos - secondFetchedExpect.Pos))
                {
                    secondFetchedExpect = expect;
                }
            }
            if(secondFetchedExpect == null || Judgement.GetGrade(inputTime - secondFetchedExpect.Time) is NoteGrade.FastGreat or NoteGrade.FastFar or NoteGrade.Miss)
                return (fetchedExpect, inputTime - fetchedExpect.Time);
            return (secondFetchedExpect, inputTime - secondFetchedExpect.Time);
        }*/

        
        var fetchedExpects = FetchSomeNotes(inputPos, inputTime, range, defaultTolerance);
        NoteExpect fetchedExpect = null;
        for(int i = 0; i < fetchedExpects.Count; i++)
        {
            var expect = fetchedExpects[i].Item1;
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

            if(i != 0 && Judgement.GetGrade(inputTime - expect.Time) is NoteGrade.FastGreat or NoteGrade.FastFar or NoteGrade.Miss) continue;

            fetchedExpect ??= expect;
            if (expect.Time <= fetchedExpect.Time
             && Vector2.SqrMagnitude(inputPos - expect.Pos) < Vector2.SqrMagnitude(inputPos - fetchedExpect.Pos))
            {
                fetchedExpect = expect;
            }
        }
        if(fetchedExpect == null) return default;
        return (fetchedExpect, inputTime - fetchedExpect.Time);
    }

    /// <summary>
    /// 入力の範囲に適合するノーツを返します
    /// </summary>
    List<(NoteExpect, float)> FetchSomeNotes(Vector2 inputPos, float inputTime, float range, float tolerance)
    {
        fetchedExpects.Clear();
        foreach(var expect in allExpects)
        {
            // 入力時間とノーツの着地予定時間を比較
            float delta = inputTime - expect.Time;
            if(Mathf.Abs(delta) > tolerance) continue;

            // 入力座標から遠かったらスルー
            if(judge.IsNearPosition(inputPos, expect.Pos, range) == false) continue;
            fetchedExpects.Add((expect, delta));
        }
        fetchedExpects.Sort((a, b) => a.Item1.Time.CompareTo(b.Item1.Time));
        return fetchedExpects;
    }

    void ProcessHold(IEnumerable<Input> inputs)
    {
        for(int i = 0; i < holds.Count; i++)
        {
            var hold = holds[i];
            if(hold.State is HoldState.None or HoldState.Idle)
            {
                throw new Exception();
            }
            else if(hold.State is HoldState.Holding)
            {
                bool isInput = 
                    (inputs != null && inputs.Any(i => judge.IsNearPosition(i.pos, hold.GetLandingPos())))
                     || isAuto;
                if(isInput)
                {
                    // ギリギリまで取らなくても判定されるように
                    if(metronome.CurrentTime > hold.EndTime - 0.2f)
                    {
                        hold.State = HoldState.Got;
                        judge.AddCombo();
                    }
                }
                else
                {
                    hold.State = HoldState.Missed;
                    judge.ResetCombo();
                }
            }
            else if(hold.State is HoldState.Missed)
            {
                if(metronome.CurrentTime > hold.EndTime)
                {
                    hold.gameObject.SetActive(false);
                    holds.RemoveAt(i);
                }
            }
            else if(hold.State is HoldState.Got)
            {
                // ちょっと早めに表示
                if(metronome.CurrentTime > hold.EndTime - 0.02f)
                {
                    hold.gameObject.SetActive(false);
                    holds.RemoveAt(i);
                    judge.PlayParticle(hold.Grade, hold.GetLandingPos());
                }
            }
        }
    }

    void ProcessArc(IEnumerable<Input> inputs)
    {
        for(int i = 0; i < arcs.Count; i++)
        {
            var arc = arcs[i];
            var arcPos = arc.GetPos();
            if(arc.GetPos().z < 0) continue; // まだ到達していない
            if(arcPos.z > arc.LastZ + 1) // アークが完全に通り過ぎた
            {
                arcs.RemoveAt(i);
                arc.SetActive(false);
                judge.RemoveLink(arc);
                continue;
            }

            // 距離の近いアークを調べる
            arc.IsPosDuplicated = false;
            var worldPos = arc.GetAnyPointOnZPlane(0);
            foreach(var otherArc in arcs)
            {
                var otherArcZ = otherArc.GetPos().z;
                if(otherArc == arc || otherArcZ < 0 || otherArcZ > otherArc.LastZ) continue;
                var otherWorldPos = otherArc.GetAnyPointOnZPlane(0);       
                if(Vector2.SqrMagnitude(worldPos - otherWorldPos) < arcDuplicateSqrDistance)
                {
                    arc.IsPosDuplicated = true;
                    otherArc.IsPosDuplicated = true;
                }
            }
            
            bool isHold = isAuto;
            if(inputs != null)
            {
                foreach(var input in inputs)
                {
                    if(judge.IsNearPosition(input.pos, worldPos, arcRange) == false) continue;
                    
                    if(arc.IsPosDuplicated)
                    {
                        isHold = true;
                        arc.FingerIndex = -1;
                        break;
                    }

                    /*if(arc.IsInvalid)
                    {
                        break;
                    }*/

                    if(arc.FingerIndex == -1)
                    {
                        isHold = true;
                        arc.FingerIndex = input.index;
                    }
                    else if(arc.FingerIndex == input.index)
                    {
                        isHold = true;
                    }
                    else
                    {
                        arc.InvalidArcJudgeAsync().Forget();
                    }
                }
            }
            
            /*foreach(var status in inputStatuses)
            {
                if(judge.IsNearPosition(status.Position, worldPos, arcRange) == false) continue;
                
                if(arc.IsPosDuplicated)
                {
                    isHold = true;
                    status.SetArcColorType(ArcColorType.None);
                    arc.FingerIndex = -1;
                    break;
                }

                if(arc.IsInvalid)
                {
                    break;
                }

                if(status.ArcColorType == ArcColorType.None)
                {
                    isHold = true;
                    status.SetArcColorType(arc.ColorType);
                    arc.FingerIndex = status.FingerIndex;
                }
                else if(status.ArcColorType == arc.ColorType)
                {
                    isHold = true;
                }
                else
                {
                    arc.InvalidArcJudgeAsync().Forget();
                }
            }*/

            if(arc.IsInvalid == false)
            {
                judge.SetShowLight(arc, worldPos, isHold);
            }
            arc.SetInput(isHold);

            var arcJ = arc.GetCurrentJudge();
            if (arcJ == null) continue; // 最後の判定を終えた
            if (arcJ.EndPos.z < arcPos.z)
            {
                arcJ.State = ArcJudgeState.Miss;
                arc.JudgeIndex++;
                judge.ResetCombo();
            }

            if ((arcJ.StartPos.z < arcPos.z && arcPos.z < arcJ.EndPos.z) == false) continue; // 判定の範囲外

            if(arcJ.State is ArcJudgeState.None)
            {
                throw new Exception();
            }
            else if(arcJ.State is ArcJudgeState.Idle && isHold && arc.IsInvalid == false)
            {
                arcJ.State = ArcJudgeState.Got;
                judge.PlayParticle(NoteGrade.Perfect, worldPos);
                arc.JudgeIndex++;
                judge.AddCombo();
            }
        }
    }
}
