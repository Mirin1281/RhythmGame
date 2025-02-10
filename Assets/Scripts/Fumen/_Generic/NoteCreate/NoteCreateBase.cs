using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    public abstract class NoteCreateBase<T> : CommandBase where T : struct, INoteData
    {
        [SerializeField] protected Mirror mirror;
        [SerializeField] float speedRate = 1f;

        /// <summary>
        /// NoteData内のWaitの加算
        /// </summary>
        protected Lpb WaitDelta { get; private set; }

        protected override float Speed => base.Speed * speedRate;

        protected abstract T[] NoteDatas { get; }
        protected abstract void Move(RegularNote note, T data);

        protected override async UniTaskVoid ExecuteAsync()
        {
            WaitDelta = default;
            foreach (var data in NoteDatas)
            {
                await Wait(data.Wait);
                WaitDelta += data.Wait;
                CreateNote(data);
            }


            void CreateNote(in T noteData)
            {
                var type = noteData.Type;
                if (type is RegularNoteType.Normal or RegularNoteType.Slide)
                {
                    RegularNote note = Helper.GetRegularNote(type);
                    Move(note, noteData);
                }
                else if (type is RegularNoteType.Hold)
                {
                    if (noteData.Length == default)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    HoldNote hold = Helper.GetHold(noteData.Length * Speed);
                    Vector3 startPos = mirror.Conv(new Vector3(noteData.X, StartBase));
                    hold.SetMaskPos(new Vector2(startPos.x, 0));
                    Move(hold, noteData);
                }
            }
        }

        /// <summary>
        /// 指定したタイミングで処理ができるWhileYieldAsync
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <param name="timings">タイミングの配列</param>
        /// <param name="timingAction">引数は(インデックス, 時間, 理想の時間との差)</param>
        /// <param name="waitDelta"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected async UniTask WhileYieldGroupAsync(
            float time, Action<float> action, Lpb[] timings, Action<(int index, float t, float d)> timingAction, float delta = -1)
        {
            Lpb next = timings[0] - WaitDelta;
            int index = 1;
            while (next.Time < 0 && index < timings.Length)
            {
                next += timings[index];
                index++;
            }

            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (t < time)
            {
                t = CurrentTime - baseTime;
                action.Invoke(t);
                if (t >= next.Time)
                {
                    if (index < timings.Length)
                    {
                        float d = t - next.Time;
                        next += timings[index];
                        timingAction.Invoke((index, t, d));
                        index++;
                    }
                    else
                    {
                        next = new Lpb(float.MinValue);
                    }
                }
                await Yield();
            }
            action.Invoke(time);
        }

        protected UniTask WhileYieldGroupAsync(
            float time, Action<float> action, int loopCount, Lpb loopWait, Action<(int index, float t, float d)> timingAction, float delta = -1)
        {
            Lpb[] timings = new Lpb[loopCount];
            for (int i = 0; i < timings.Length; i++)
            {
                timings[i] = loopWait;
            }
            return WhileYieldGroupAsync(time, action, timings, timingAction, delta);

            /*int index = 0;
            Lpb next = loopWait - WaitDelta;
            while (next.Time < 0 && index < loopCount)
            {
                next += loopWait;
                index++;
            }

            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (t < time)
            {
                t = CurrentTime - baseTime;
                action.Invoke(t);
                if (t >= next.Time)
                {
                    if (index < loopCount)
                    {
                        float d = t - next.Time;
                        next += loopWait;
                        timingAction.Invoke((index, t, d));
                        index++;
                    }
                    else
                    {
                        next = new Lpb(float.MinValue);
                    }
                }
                await Helper.Yield();
            }
            action.Invoke(time);*/
        }


#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            int noteCount = NoteDatas == null ? 0 : NoteDatas.Length;
            return new Color32(
                255,
                (byte)Mathf.Clamp(246 - noteCount * 2, 96, 246),
                (byte)Mathf.Clamp(230 - noteCount * 2, 130, 230),
                255);
        }

        protected override string GetSummary()
        {
            return NoteDatas?.Length + mirror.GetStatusText();
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.BeatDelta);
        }
        public override void OnPeriod()
        {
            DebugPreview();
        }

        void DebugPreview(bool beforeClear = true, int beatDelta = 1)
        {
            CommandEditorUtility.DebugPreview2DNotes(NoteDatas, Helper, mirror, beforeClear, beatDelta);
        }
#endif
    }
}
