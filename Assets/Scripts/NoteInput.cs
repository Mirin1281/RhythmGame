using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HoldState = HoldNote.InputState;
using ArcJudgeState = ArcJudge.InputState;
using InputStatus = InputManager.InputStatus;
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
    [SerializeField] bool isAuto;

    readonly List<NoteExpect> allExpects = new(63);
    readonly List<HoldNote> holds = new(4);
    readonly List<ArcNote> arcs = new(4);

    static readonly float defaultRange = 2.4f;
    static readonly float flickRange = 3f;
    static readonly float arcRange = 2f;

    static readonly float normalTolerance = 0.1f;
    static readonly float slideTolerance = 0.25f;

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
    }
    void OnDestroy()
    {
        inputManager.OnInput -= OnInput;
        inputManager.OnHold -= OnHold;
        inputManager.OnFlick -= OnFlick;
        inputManager.OnUp -= OnUp;
    }

    void OnUp(int fingerIndex)
    {
        foreach(var arc in arcs)
        {
            if(arc.FingerIndex == fingerIndex && arc.IsInvalid == false)
            {
                arc.InvalidArcJudgeAsync().Forget();
            }
        }
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
            ProcessHold(inputManager.InputStatuses);
            ProcessArc(inputManager.InputStatuses);
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
                    var delta = metronome.CurrentTime - expect.Time;
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    HoldNote hold = AddHold(expect);
                    hold.Grade = NoteGrade.Perfect;
                    RemoveExpect(expect, false);
                    judge.AddCombo();
                }
                else
                {
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    RemoveExpect(expect);
                    judge.AddCombo();
                }
            }
            else if(metronome.CurrentTime > expect.Time + 0.18f)
            {
                // 遅れたらノーツを除去
                if(expect.Note.Type == NoteType.Hold)
                {
                    AddHold(expect);
                    RemoveExpect(expect, false);
                    judge.ResetCombo();
                }
                else
                {
                    RemoveExpect(expect);
                    judge.ResetCombo();
                }
            }
        }
    }

    void OnInput(Vector2 pos)
    {
        (NoteExpect expect, float delta) = FetchNearestNote(pos, metronome.CurrentTime, defaultRange, NoteType.Normal, NoteType.Hold);
        if(expect == null) return;

        NoteGrade grade = judge.GetGradeAndSetText(delta);
        if(grade == NoteGrade.Miss)
        {
            judge.ResetCombo();
            RemoveExpect(expect);
            return;
        }
        if(expect.Note.Type == NoteType.Hold)
        {
            var hold = AddHold(expect);
            hold.Grade = grade;
            RemoveExpect(expect, false);
            judge.AddCombo();
            judge.PlayParticle(grade, expect.Pos);
            return;
        }
        RemoveExpect(expect);
        judge.AddCombo();
        judge.PlayParticle(grade, expect.Pos);
    }

    void OnHold(List<InputStatus> inputStatuses)
    {
        if(isAuto == false)
        {
            ProcessHold(inputStatuses);
            ProcessArc(inputStatuses);
        }
        
        foreach(var status in inputStatuses)
        {
            List<(NoteExpect, float)> expects = FetchSomeNotes(status.Position, metronome.CurrentTime, 2.4f, NoteType.Slide);
            if(expects == null) continue;

            foreach(var (expect, delta) in expects)
            {
                RemoveExpect(expect, false);
                UniTask.Void(async () => 
                {
                    if(delta < 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(-delta), cancellationToken: destroyCancellationToken);
                    }
                    
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    expect.Note.gameObject.SetActive(false);
                    judge.AddCombo();
                });
            }
        }
    }
    
    void OnFlick(Vector2 pos)
    {
        List<(NoteExpect, float)> expects = FetchSomeNotes(pos, metronome.CurrentTime, flickRange, NoteType.Flick);
        if(expects == null) return;

        foreach(var (expect, delta) in expects)
        {
            RemoveExpect(expect, false);
            UniTask.Void(async () => 
            {
                if(delta < 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(-delta), cancellationToken: destroyCancellationToken);
                }
                
                judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                expect.Note.gameObject.SetActive(false);
                judge.AddCombo();
            });
        }
    }

    (NoteExpect, float) FetchNearestNote(Vector2 inputPos, float inputTime, float range, params NoteType[] fetchTypes)
    {
        NoteExpect fetchedExpect = null;
        float time = Mathf.Infinity;
        foreach(var expect in allExpects)
        {
            // inputTimeSampleと落ちてくる時間を比較、近いものを選定
            float delta = inputTime - expect.Time;
            if (Mathf.Abs(delta) > normalTolerance) continue;

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
            if(judge.IsNearPosition(inputPos, expect.Pos, range) == false) continue;

            // より早く落ちてくるノーツを判定(複数あればよりinputPosに近いノーツを返す)
            if(fetchedExpect == null)
            {
                fetchedExpect = expect;
            }
            float t = expect.Time;
            if (t <= time
             && Vector2.SqrMagnitude(inputPos - expect.Pos) < Vector2.SqrMagnitude(inputPos - fetchedExpect.Pos))
            {
                time = t;
                fetchedExpect = expect;
            }
        }
        if(fetchedExpect == null) return default;

        return (fetchedExpect, inputTime - fetchedExpect.Time);
    }

    readonly List<(NoteExpect, float)> fetchedExpects = new(8);
    List<(NoteExpect, float)> FetchSomeNotes(Vector2 inputPos, float inputTime, float range, params NoteType[] fetchTypes)
    {
        fetchedExpects.Clear();
        foreach(var expect in allExpects)
        {
            float delta = inputTime - expect.Time;
            if(Mathf.Abs(delta) > slideTolerance) continue;

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

            if(judge.IsNearPosition(inputPos, expect.Pos, range) == false) continue;
            fetchedExpects.Add((expect, delta));
        }
        if(fetchedExpects.Count == 0) return default;
        return fetchedExpects;
    }

    void ProcessHold(List<InputStatus> inputStatuses)
    {
        if(holds.Count == 0) return;
        for(int i = 0; i < holds.Count; i++)
        {
            var hold = holds[i];
            if(hold.State is HoldState.None or HoldState.Idle)
            {
                throw new Exception();
            }
            else if(hold.State is HoldState.Holding)
            {
                bool isInput = inputStatuses.Any(status => judge.IsNearPosition(status.Position, hold.GetLandingPos()));
                if(isInput || isAuto)
                {
                    // ギリギリまで取らなくても判定されるように
                    if(metronome.CurrentTime > hold.EndTime - 0.16f)
                    {
                        judge.AddCombo();
                        hold.State = HoldState.Got;
                    }
                }
                else
                {
                    hold.State = HoldState.Missed;
                    judge.ResetCombo();
                }
            }
            else if(hold.State == HoldState.Missed)
            {
                if(metronome.CurrentTime > hold.EndTime)
                {
                    hold.gameObject.SetActive(false);
                    holds.RemoveAt(i);
                }
            }
            else if(hold.State == HoldState.Got)
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

    void ProcessArc(List<InputStatus> inputStatuses)
    {
        for(int i = 0; i < arcs.Count; i++)
        {
            var arc = arcs[i];
            var arcPos = arc.GetPos();
            if(arcPos.z < 0) continue; // まだ到達していない
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
            foreach(var status in inputStatuses)
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
            }

            judge.SetShowLight(arc, worldPos, isHold);
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
            else if(arcJ.State is ArcJudgeState.Idle && isHold)
            {
                arcJ.State = ArcJudgeState.Got;
                judge.PlayParticle(NoteGrade.Perfect, worldPos);
                arc.JudgeIndex++;
                judge.AddCombo();
            }
        }
    }
}
