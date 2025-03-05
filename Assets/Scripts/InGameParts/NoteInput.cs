using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Input = InputManager.Input;
using System.Linq;

namespace NoteCreating
{
    using HoldState = HoldNote.InputState;
    using ArcJudgeState = ArcJudge.InputState;
    using ExpectType = NoteJudgeStatus.ExpectType;

    public class NoteJudgeStatus
    {
        /// <summary>
        /// ノーツの着地点を自動で予測する
        /// </summary>
        public enum ExpectType
        {
            Static, // 座標固定
            Y_Static, // X座標は動的予測
            Dynamic, // ノーツの座標を反映
        }

        public readonly RegularNote Note;

        readonly RegularNoteType noteType;
        public RegularNoteType NoteType
        {
            get
            {
                if (Note == null)
                {
                    return noteType;
                }
                else
                {
                    return Note.Type;
                }
            }
        }

        readonly Vector2 pos;
        public Vector2 Pos => Type switch
        {
            ExpectType.Static => pos,
            ExpectType.Y_Static => new Vector2(Note.GetPos(true).x, pos.y),
            ExpectType.Dynamic => Note.GetPos(true),
            _ => throw new Exception()
        };

        readonly float rot;
        public float Rot
        {
            get
            {
                if (Note == null)
                {
                    return rot;
                }
                else
                {
                    return Note.GetRot(true);
                }
            }
        }

        readonly bool isVerticalRange;
        public bool IsVerticalRange
        {
            get
            {
                if (Note == null)
                {
                    return isVerticalRange;
                }
                else
                {
                    return Note.IsVerticalRange;
                }
            }
        }

        public float Time { get; set; }
        public float HoldEndTime { get; set; }
        public readonly ExpectType Type;
        public bool IsMute { get; set; }

        public void DisableActive()
        {
            if (Note == null) return;
            Note.SetActive(false);
        }

        public NoteJudgeStatus(RegularNote note, Vector2 pos, float time, Lpb holdEndTime = default, ExpectType expectType = ExpectType.Y_Static, bool isMute = false)
        {
            Note = note;
            this.pos = pos;
            Time = time;
            HoldEndTime = holdEndTime.Time;
            Type = expectType;
            this.IsMute = isMute;
        }

        public NoteJudgeStatus(RegularNoteType noteType, Vector2 pos, float time, bool isVerticalRange = false, float rot = 0)
        {
            if (noteType is RegularNoteType._None or RegularNoteType.Hold)
            {
                Debug.LogWarning("RegularNoteTypeが適切ではありません");
            }
            this.noteType = noteType;
            this.pos = pos;
            Time = time;
            Type = ExpectType.Static;
            this.isVerticalRange = isVerticalRange;
            this.rot = rot;
        }
    }

    public class NoteInput : MonoBehaviour
    {
        [SerializeField] Judgement judge;
        [SerializeField] ArcLightOperator lightOperator;
        [SerializeField] PoolManager poolManager;
#if UNITY_EDITOR
        [SerializeField] bool isAuto;
#else
    bool isAuto = false;
#endif

        readonly List<NoteJudgeStatus> allStatuses = new(63);
        readonly List<HoldNote> holds = new(4);
        readonly List<ArcNote> arcs = new(4);
        readonly List<(NoteJudgeStatus status, float delta)> fetchedStatuses = new(8);

        static readonly float defaultTolerance = 0.15f;
        static readonly float wideTolerance = 0.25f;

        float CurrentTime => Metronome.Instance.CurrentTime;

        void Start()
        {
            if (isAuto) return;
            var inputManager = FindAnyObjectByType<InputManager>();
            inputManager.OnDown += OnDown;
            inputManager.OnHold += OnHold;
            inputManager.OnUp += OnUp;


            async UniTaskVoid OnUp(Input input)
            {
                await UniTask.DelayFrame(2, cancellationToken: destroyCancellationToken);
                foreach (var arc in arcs)
                {
                    if (arc.FingerIndex == input.index && arc.IsInvalid == false)
                    {
                        arc.InvalidArcJudgeAsync().Forget();
                    }
                }
            }
        }

        /// <summary>
        /// ノーツの打点情報を伝えます
        /// </summary>
        ///
        /// ノーツ, 何秒後に打点するか, ホールド時間(ホールドのみ), 同時押しの検知をするか, 同時押しの検知をするか
        public void AddExpect(RegularNote note, float time, Lpb holdingTime = default, ExpectType expectType = ExpectType.Y_Static, bool isCheckSimultaneous = true)
        {
            AddExpect(new NoteJudgeStatus(note, new Vector2(default, 0), time, holdingTime, expectType), isCheckSimultaneous);
        }
        public void AddExpect(NoteJudgeStatus judgeStatus, bool isCheckSimultaneous = true)
        {
            float absoluteTime = CurrentTime + judgeStatus.Time;
            judgeStatus.Time = absoluteTime;
            judgeStatus.HoldEndTime += absoluteTime;
            if (isCheckSimultaneous)
            {
                foreach (var e in allStatuses)
                {
                    if (Mathf.Approximately(absoluteTime, e.Time))
                    {
                        poolManager.SetMultitapSprite(judgeStatus.Note);
                        poolManager.SetMultitapSprite(e.Note);
                    }
                }
            }
            allStatuses.Add(judgeStatus);
        }

        void AddHold(HoldNote hold, float holdEndTime, bool missed = false)
        {
            hold.State = missed ? HoldState.Missed : HoldState.Holding;
            hold.EndTime = holdEndTime;
            holds.Add(hold);
        }

        public void AddArc(ArcNote arc)
        {
            arcs.Add(arc);
        }

        void PlayNoteSE(RegularNoteType noteType)
        {
            var volume = RhythmGameManager.GetNoteVolume();
            if (volume == 0f || RhythmGameManager.Setting.IsNoteMute) return;
            if (noteType == RegularNoteType.Slide)
            {
                SEManager.Instance.PlayNoteSE(SEType.my2_low);
            }
            else
            {
                SEManager.Instance.PlayNoteSE(SEType.my2);
            }
        }

        void Update()
        {
            if (isAuto)
            {
                ProcessHold(null);
                ProcessArc(null);
            }

            for (int i = 0; i < allStatuses.Count;)
            {
                var status = allStatuses[i];
                if (isAuto && CurrentTime > status.Time)
                {
                    // オート
                    if (status.NoteType == RegularNoteType.Hold)
                    {
                        AddHold(status.Note as HoldNote, status.HoldEndTime);
                    }
                    else
                    {
                        status.DisableActive();
                    }

                    allStatuses.Remove(status);
                    judge.PlayParticle(NoteGrade.Perfect, status.Pos);
                    judge.SetCombo(NoteGrade.Perfect);
#if UNITY_EDITOR
                    judge.DebugShowRange(status);
#endif
                    if (status.IsMute == false)
                        PlayNoteSE(status.NoteType);
                }
                else if (CurrentTime > status.Time + 0.12f)
                {
                    // 遅れたらノーツを除去
                    if (status.NoteType == RegularNoteType.Hold)
                    {
                        AddHold(status.Note as HoldNote, status.HoldEndTime, missed: true);
                    }
                    allStatuses.Remove(status);
                    judge.SetCombo(NoteGrade.Miss);
                    if (status.Note != null)
                    {
                        status.Note.OnMiss();
                    }
                }
                else
                {
                    i++;
                }
            }
        }

        void OnDown(Input input)
        {
            (NoteJudgeStatus status, float delta) = FetchNearestNote(input.pos, CurrentTime, RegularNoteType.Normal, RegularNoteType.Hold);
            if (status == null) return;

            NoteGrade grade = judge.GetGradeAndSetText(delta);
            judge.SetCombo(grade);
            if (grade == NoteGrade.Miss && status.NoteType != RegularNoteType.Hold) // ホールドは判定が2つあるので除外
            {
                allStatuses.Remove(status);
                status.DisableActive();
                return;
            }

            judge.PlayParticle(grade, status.Pos);
            allStatuses.Remove(status);
            if (status.NoteType == RegularNoteType.Hold)
            {
                AddHold(status.Note as HoldNote, status.HoldEndTime);
            }
            else
            {
                status.DisableActive();
            }

            if (status.IsMute == false)
                PlayNoteSE(status.NoteType);
        }

        void OnHold(List<Input> inputs)
        {
            if (isAuto == false)
            {
                ProcessHold(inputs);
                ProcessArc(inputs);
            }

            foreach (var input in inputs)
            {
                List<(NoteJudgeStatus, float)> statuses = FetchSomeNotes(input.pos, CurrentTime, wideTolerance);
                if (statuses == null) continue;

                foreach (var (status, delta) in statuses)
                {
                    if (status.NoteType != RegularNoteType.Slide) continue;
                    allStatuses.Remove(status);
                    UniTask.Void(async () =>
                    {
                        if (delta < 0)
                        {
                            await MyUtility.WaitSeconds(-delta, destroyCancellationToken);
                        }
                        judge.PlayParticle(NoteGrade.Perfect, status.Pos);
                        if (status.IsMute == false)
                            PlayNoteSE(status.NoteType);
                        status.DisableActive();
                        judge.SetCombo(NoteGrade.Perfect);
                    });
                }
            }
        }

        /// <summary>
        /// 条件に適合するノーツを1つ返します
        /// </summary>
        (NoteJudgeStatus, float) FetchNearestNote(Vector2 inputPos, float inputTime, params RegularNoteType[] fetchTypes)
        {
            var fetchedStatuses = FetchSomeNotes(inputPos, inputTime, defaultTolerance);
            NoteJudgeStatus fetchedStatus = null;
            for (int i = 0; i < fetchedStatuses.Count; i++)
            {
                var status = fetchedStatuses[i].Item1;
                bool isMatch = false;
                foreach (var type in fetchTypes)
                {
                    if (status.NoteType == type)
                    {
                        isMatch = true;
                        break;
                    }
                }
                if (isMatch == false) continue;

                if (i != 0 && Judgement.GetGrade(inputTime - status.Time) is NoteGrade.FastGreat or NoteGrade.FastFar or NoteGrade.Miss) continue;

                fetchedStatus ??= status;
                if (status.Time <= fetchedStatus.Time
                 && Vector2.SqrMagnitude(inputPos - status.Pos) < Vector2.SqrMagnitude(inputPos - fetchedStatus.Pos))
                {
                    fetchedStatus = status;
                }
            }
            if (fetchedStatus == null) return default;
            return (fetchedStatus, inputTime - fetchedStatus.Time);
        }

        /// <summary>
        /// 入力の範囲に適合するノーツを全て返します
        /// </summary>
        List<(NoteJudgeStatus, float)> FetchSomeNotes(Vector2 inputPos, float inputTime, float tolerance)
        {
            fetchedStatuses.Clear();

            foreach (var status in allStatuses)
            {
                // 入力時間とノーツの着地予定時間を比較
                float delta = inputTime - status.Time;
                if (Mathf.Abs(delta) > tolerance) continue;

                // 入力座標から遠かったらスルー
                if (judge.IsNearPosition(status, inputPos) == false) continue;
                fetchedStatuses.Add((status, delta));
            }
            fetchedStatuses.Sort((a, b) => a.status.Time.CompareTo(b.status.Time));
            return fetchedStatuses;
        }

        void ProcessHold(IEnumerable<Input> inputs)
        {
            for (int i = 0; i < holds.Count; i++)
            {
                var hold = holds[i];
                if (hold.State is HoldState.Idle)
                {
                    Debug.LogError("hold.State is HoldState.Idle");
                }
                else if (hold.State is HoldState.Holding)
                {
                    bool isInput = inputs != null && inputs.Any(input => judge.IsNearPositionHold(hold, input.pos));
                    if (isInput || isAuto)
                    {
                        hold.NoInputTime = 0f;
                        if (CurrentTime > hold.EndTime - 0.2f) // ギリギリまで取らなくても判定されるように
                        {
                            hold.State = HoldState.Get;
                            judge.SetCombo(NoteGrade.Perfect);
                        }
                    }
                    else
                    {
                        hold.NoInputTime += Time.deltaTime;
                        if (hold.NoInputTime > 0.1f) // 一瞬離しても許容
                        {
                            hold.State = HoldState.Missed;
                            hold.OnMiss();
                            judge.SetCombo(NoteGrade.Miss);
                        }
                    }
                }
                else if (hold.State is HoldState.Get)
                {
                    // ちょっと早めに表示
                    if (CurrentTime > hold.EndTime - 0.02f)
                    {
                        if (hold.State == HoldState.Get)
                        {
                            judge.PlayParticle(NoteGrade.Perfect, hold.GetLandingPos());
                            hold.State = HoldState.Final;
                        }

                        if (CurrentTime > hold.EndTime)
                        {
                            hold.SetActive(false);
                            holds.RemoveAt(i);
                        }
                    }
                }
                else //if (hold.State is HoldState.Missed)
                {
                    if (CurrentTime > hold.EndTime)
                    {
                        hold.SetActive(false);
                        holds.RemoveAt(i);
                    }
                }
            }
        }

        void ProcessArc(IEnumerable<Input> inputs)
        {
            for (int i = 0; i < arcs.Count; i++)
            {
                var arc = arcs[i];

                // アークの一部分が判定に入っていなければここで弾かれる
                if (arc.IsReached() == false) continue; // まだ到達していない
                else if (arc.TailY < -3) // アークが完全に通り過ぎた
                {
                    arcs.RemoveAt(i);
                    arc.SetActive(false);
                    lightOperator.RemoveLink(arc);
                    continue;
                }
                else if (arc.TailY < 0) // アークの終端が通り過ぎた
                {
                    continue;
                }

                bool isOverlappable = arc.IsOverlapped();
                var landingPos = arc.GetPointOnYPlane(0) + arc.GetPos();
                landingPos = new Vector3(landingPos.x, 0);

                // 入力が範囲内にあるかなどを判定
                bool isHold = isAuto;
                if (inputs != null)
                {
                    foreach (var input in inputs)
                    {
                        if (arc.IsInvalid) break; // 無効か
                        if (judge.IsNearPositionArc(input.pos, landingPos) == false) continue; // 入力と近いか

                        if (isOverlappable) // オーバーラップだったら押下
                        {
                            isHold = true;
                            arc.FingerIndex = -1;
                            break;
                        }

                        if (arc.FingerIndex == -1) // 指がデフォルトだったら登録して押下
                        {
                            isHold = true;
                            arc.FingerIndex = input.index;
                        }
                        else if (arc.FingerIndex == input.index) // 指が同じだったら押下
                        {
                            isHold = true;
                        }
                        else // 指が一致しなかったら一定時間判定無効(ミス)
                        {
                            arc.InvalidArcJudgeAsync().Forget();
                            break;
                        }
                    }
                }

                lightOperator.SetShowLight(arc, landingPos, isHold);
                arc.IsHold = isHold;

                var arcJ = arc.GetCurrentJudge();
                float headY = arc.HeadY;
                if (arcJ == null)
                {
                    continue;
                }
                else if (headY < -arcJ.EndPos.z) // 判定の終端を過ぎたらミス(再終端のミス判定のためにバッファ)
                {
                    arcJ.State = ArcJudgeState.Miss;
                    arc.JudgeIndex++;
                    judge.SetCombo(NoteGrade.Miss);
                    continue;
                }
                else if (headY > -arcJ.StartPos.z) // まだ次の判定が来ていないときはスルー
                {
                    continue;
                }

                if (arcJ.State is ArcJudgeState.None)
                {
                    throw new Exception();
                }
                else if (arcJ.State is ArcJudgeState.Idle && isHold)
                {
                    if (arc.JudgeIndex == 0) PlayNoteSE(RegularNoteType.Normal);
                    arcJ.State = ArcJudgeState.Get;
                    judge.PlayParticle(NoteGrade.Perfect, landingPos);
                    judge.SetCombo(NoteGrade.Perfect);
                    arc.JudgeIndex++;
                }
            }
        }
    }
}
