using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    // 作り方のコツ
    // 1. 非同期ループin非同期ループは使わない
    // 2. 状態をenumなどで管理し、都度振る舞いを設定する
    // 3. インデックス変数は0スタート、最後にインクリメントする(インクリメントは忘れやすいので注意)
    [AddTypeMenu(FumenPathContainer.NoteCreate + "急停止 & 急加速", -50), System.Serializable]
    public class F_StopAndGo : NoteCreateBase<NoteData>
    {
        [System.Serializable]
        struct StopData
        {
            [SerializeField] Lpb timing;
            [SerializeField] Lpb stopWait;
            public readonly Lpb Timing => timing;
            public readonly Lpb StopWait => stopWait;
            public StopData(Lpb timing = default, Lpb stopWait = default)
            {
                this.timing = timing;
                this.stopWait = stopWait;
            }
        }

        enum MoveStatus { Default, Stop, Accel }

        [SerializeField] float acceleration = 3;
        [SerializeField] StopData[] stopDatas = new StopData[] { new(new Lpb(4) * 6, new Lpb(4)) };
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }

            AddExpect();
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            Lpb w = WaitDelta;

            float currentTiming = -w.Time;
            float nextTiming = (stopDatas.Length == 0 ? Lpb.Infinity : stopDatas[0].Timing).Time - w.Time;
            float stopWait = default;

            int i = 0;
            MoveStatus status = MoveStatus.Default;

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                if (status == MoveStatus.Default && t > nextTiming)
                {
                    if (i >= stopDatas.Length)
                    {
                        nextTiming = Lpb.Infinity.Time;
                    }
                    else
                    {
                        status = MoveStatus.Stop;
                        currentTiming += stopDatas[i].Timing.Time;
                        if (i + 1 >= stopDatas.Length)
                        {
                            nextTiming += Lpb.Infinity.Time;
                        }
                        else
                        {
                            nextTiming += stopDatas[i + 1].Timing.Time;
                        }
                        stopWait = stopDatas[i].StopWait.Time;
                    }
                }

                float t2 = t;
                if (status == MoveStatus.Stop)
                {
                    t2 = currentTiming;
                    if (t >= currentTiming + stopWait * (1f - 1f / acceleration))
                    {
                        status = MoveStatus.Accel;
                    }
                }
                if (status == MoveStatus.Accel)
                {
                    float rate = (t - (currentTiming + stopWait * (1f - 1f / acceleration))) / stopWait * acceleration;
                    t2 = Mathf.Lerp(currentTiming, currentTiming + stopWait, rate);
                    if (t >= currentTiming + stopWait)
                    {
                        t2 = currentTiming + stopWait;
                        status = MoveStatus.Default;
                        i++;
                    }
                }

                var pos = mirror.Conv(new Vector3(data.X, (MoveTime - t2) * Speed));
                note.SetPos(pos);
            });
        }

#if UNITY_EDITOR
        protected override string GetName()
        {
            return "Stop&Go";
        }
#endif
    }
}