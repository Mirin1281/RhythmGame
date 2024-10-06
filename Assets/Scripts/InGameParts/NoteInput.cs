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
    public enum ExpectMode
    {
        Static,
        Y_Static,
        Dynamic,
    }
    public readonly NoteBase Note;
    Vector2 pos;
    public Vector2 Pos => Mode switch
    {
        ExpectMode.Static => pos,
        ExpectMode.Y_Static => new Vector2(Note.GetPos(true).x, pos.y),
        ExpectMode.Dynamic => Note.GetPos(true),
        _ => throw new Exception()
    };
    public readonly float Time;
    public readonly float HoldEndTime;
    public readonly ExpectMode Mode;

    public NoteExpect(NoteBase note, Vector2 pos, float time, float holdEndTime = 0, ExpectMode mode = ExpectMode.Static)
    {
        Note = note;
        this.pos = pos;
        Time = time;
        HoldEndTime = holdEndTime;
        Mode = mode;
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

    static readonly float defaultTolerance = 0.1f;
    static readonly float wideTolerance = 0.25f;

    static readonly float arcDuplicateSqrDistance = 8f;

    void Start()
    {
#if UNITY_EDITOR
#else
        isAuto = false;
#endif
        if(isAuto) return;
        inputManager.OnDown += OnDown;
        inputManager.OnHold += OnHold;
        inputManager.OnUp += OnUp;
        inputManager.OnFlick += OnFlick;


        void OnUp(Input input)
        {
            UniTask.Void(async () => 
            {
                await UniTask.Yield(destroyCancellationToken);
                await UniTask.Yield(destroyCancellationToken);
                foreach(var arc in arcs)
                {
                    if(arc.FingerIndex == input.index && arc.IsInvalid == false)
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

    HoldNote AddHold(NoteExpect expect, bool isMiss = false)
    {
        var hold = expect.Note as HoldNote;
        hold.State = isMiss ? HoldState.Missed : HoldState.Holding;
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

        for(int i = 0; i < allExpects.Count; )
        {
            var expect = allExpects[i];
            if(isAuto && metronome.CurrentTime > expect.Time)
            {
                // オート
                if(expect.Note.Type == NoteType.Hold)
                {
                    AddHold(expect);
                }
                else
                {
                    expect.Note.SetActive(false);
                }
                allExpects.Remove(expect);
                judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                judge.SetCombo(NoteGrade.Perfect);
                judge.DebugShowRange(expect).Forget();
                SEManager.Instance.PlaySE(SEType.my2);
            }
            else if(metronome.CurrentTime > expect.Time + 0.12f)
            {
                // 遅れたらノーツを除去
                if(expect.Note.Type == NoteType.Hold)
                {
                    AddHold(expect, true);
                }
                allExpects.Remove(expect);
                expect.Note.OnMiss();
                judge.SetCombo(NoteGrade.Miss);
            }
            else
            {
                i++;
            }
        }
    }

    void OnDown(Input input)
    {
        (NoteExpect expect, float delta) = FetchNearestNote(
            input.pos, metronome.CurrentTime, NoteType.Normal, NoteType.Hold, NoteType.Sky);
        if(expect == null) return;

        NoteGrade grade = judge.GetGradeAndSetText(delta);
        judge.SetCombo(grade);
        if(grade == NoteGrade.Miss && expect.Note.Type != NoteType.Hold) // ホールドは判定が2つあるので除外
        {
            allExpects.Remove(expect);
            expect.Note.OnMiss();
            return;
        }

        judge.PlayParticle(grade, expect.Pos);
        SEManager.Instance.PlaySE(SEType.my2);
        allExpects.Remove(expect);
        if(expect.Note.Type == NoteType.Hold)
        {
            AddHold(expect);
        }
        else
        {
            expect.Note.SetActive(false);
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
            List<(NoteExpect, float)> expects = FetchSomeNotes(input.pos, metronome.CurrentTime, wideTolerance);
            if(expects == null) continue;

            foreach(var (expect, delta) in expects)
            {
                if(expect.Note.Type != NoteType.Slide) continue;
                allExpects.Remove(expect);
                UniTask.Void(async () => 
                {
                    if(delta < 0)
                    {
                        await MyUtility.WaitSeconds(-delta, destroyCancellationToken);
                    }
                    
                    judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                    SEManager.Instance.PlaySE(SEType.my2);
                    expect.Note.SetActive(false);
                    judge.SetCombo(NoteGrade.Perfect);
                });
            }
        }
    }
    
    void OnFlick(Input input)
    {
        List<(NoteExpect, float)> expects = FetchSomeNotes(input.pos, metronome.CurrentTime, wideTolerance);
        if(expects == null) return;

        foreach(var (expect, delta) in expects)
        {
            if(expect.Note.Type != NoteType.Flick) continue;
            allExpects.Remove(expect);
            UniTask.Void(async () => 
            {
                if(delta < 0)
                {
                    await MyUtility.WaitSeconds(-delta, destroyCancellationToken);
                }
                
                judge.PlayParticle(NoteGrade.Perfect, expect.Pos);
                SEManager.Instance.PlaySE(SEType.my2);
                expect.Note.SetActive(false);
                judge.SetCombo(NoteGrade.Perfect);
            });
        }
    }

    (NoteExpect, float) FetchNearestNote(Vector2 inputPos, float inputTime, params NoteType[] fetchTypes)
    {
        var fetchedExpects = FetchSomeNotes(inputPos, inputTime, defaultTolerance);
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
    List<(NoteExpect, float)> FetchSomeNotes(Vector2 inputPos, float inputTime, float tolerance)
    {
        fetchedExpects.Clear();
        foreach(var expect in allExpects)
        {
            // 入力時間とノーツの着地予定時間を比較
            float delta = inputTime - expect.Time;
            if(Mathf.Abs(delta) > tolerance) continue;

            
            // 入力座標から遠かったらスルー
            if(judge.IsNearPosition(expect, inputPos) == false) continue;
            
            /*if(expect.Mode == NoteExpect.ExpectMode.Static)
            {
                if(judge.IsNearPosition(expect, inputPos) == false) continue;
            }
            else if(expect.Mode == NoteExpect.ExpectMode.Y_Static)
            {
                if(judge.IsNearPosition(expect.Note, new Vector2(expect.Note.GetPos().x, expect.Pos.y), inputPos) == false) continue;
            }
            else if(expect.Mode == NoteExpect.ExpectMode.Dynamic)
            {
                if(judge.IsNearPosition(expect.Note, expect.Note.GetPos(), inputPos) == false) continue;
            }*/
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
                    (inputs != null && inputs.Any(i => judge.IsNearPositionHold(hold, i.pos)))
                     || isAuto;
                if(isInput)
                {
                    hold.NoInputTime = 0f;
                    // ギリギリまで取らなくても判定されるように
                    if(metronome.CurrentTime > hold.EndTime - 0.2f)
                    {
                        hold.State = HoldState.Got;
                        judge.SetCombo(NoteGrade.Perfect);
                    }
                }
                else
                {
                    hold.NoInputTime += Time.deltaTime;
                    if(hold.NoInputTime > 0.1f) // 一瞬離しても許容
                    {
                        hold.State = HoldState.Missed;
                        hold.OnMiss();
                        judge.SetCombo(NoteGrade.Miss);
                    }
                }
            }
            else if(hold.State is HoldState.Missed)
            {
                if(metronome.CurrentTime > hold.EndTime)
                {
                    hold.SetActive(false);
                    holds.RemoveAt(i);
                }
            }
            else if(hold.State is HoldState.Got)
            {
                // ちょっと早めに表示
                if(metronome.CurrentTime > hold.EndTime - 0.02f)
                {
                    hold.SetActive(false);
                    holds.RemoveAt(i);
                    judge.PlayParticle(NoteGrade.Perfect, hold.GetLandingPos());
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
            var arcDownPos = arc.Is2D ? -arcPos.y : arcPos.z;
            if(arcDownPos < 0) continue; // まだ到達していない
            if(arcDownPos > arc.LastZ + 1) // アークが完全に通り過ぎた
            {
                arcs.RemoveAt(i);
                arc.SetActive(false);
                judge.RemoveLink(arc);
                continue;
            }

            // 距離の近いアークを調べる
            var arcJ = arc.GetCurrentJudge();
            arc.IsPosDuplicated = arcJ == null || arcJ.IsDuplicated;
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
                    if(judge.IsNearPositionArc(input.pos, worldPos) == false) continue;
                    
                    if(arc.IsPosDuplicated)
                    {
                        isHold = true;
                        arc.FingerIndex = -1;
                        break;
                    }

                    if(arc.IsInvalid) continue;

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
            
            judge.SetShowLight(arc, worldPos, isHold);
            arc.SetInput(isHold);
            
            if (arcJ == null) continue; // 最後の判定を終えた

            if (arcJ.EndPos.z < arcDownPos)
            {
                arcJ.State = ArcJudgeState.Miss;
                arc.JudgeIndex++;
                judge.SetCombo(NoteGrade.Miss);
            }

            if ((arcJ.StartPos.z < arcDownPos && arcDownPos < arcJ.EndPos.z) == false) continue; // 判定の範囲外

            if(arcJ.State is ArcJudgeState.None)
            {
                throw new Exception();
            }
            else if(arcJ.State is ArcJudgeState.Idle && isHold && arc.IsInvalid == false)
            {
                if(arc.JudgeIndex == 0)
                    SEManager.Instance.PlaySE(SEType.my2);
                arcJ.State = ArcJudgeState.Got;
                judge.PlayParticle(NoteGrade.Perfect, worldPos);
                judge.SetCombo(NoteGrade.Perfect);
                arc.JudgeIndex++;
            }
        }
    }
}
