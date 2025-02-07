using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace NoteCreating
{
    public interface INoteData
    {
        public RegularNoteType Type { get; }
        public float X { get; }
        public float Wait { get; }
        public float Length { get; }
    }

    [Serializable]
    public struct NoteData : INoteData
    {
        [SerializeField, Min(0)] float wait;
        [SerializeField] RegularNoteType type;
        [SerializeField] float x;
        [SerializeField, Min(0)] float length;

        public readonly float Wait => wait;
        public readonly RegularNoteType Type => type;
        public readonly float X => x;
        public readonly float Length => length;

        public NoteData(float wait, RegularNoteType noteType = RegularNoteType.Normal, float x = 0, float length = 4)
        {
            this.wait = wait;
            this.type = noteType;
            this.x = x;
            this.length = length;
        }
    }

    [Serializable]
    public struct NoteDataAdvanced : INoteData
    {
        [SerializeField, Min(0)] float wait;
        [SerializeField] RegularNoteType type;
        [SerializeField] float x;
        [SerializeField, Min(0)] float length;
        [SerializeField] float option1;
        [SerializeField] float option2;

        public readonly float Wait => wait;
        public readonly RegularNoteType Type => type;
        public readonly float X => x;
        public readonly float Length => length;
        public readonly float Option1 => option1;
        public readonly float Option2 => option2;

        public NoteDataAdvanced(float wait, RegularNoteType noteType = RegularNoteType.Normal, float x = 0, float length = 4, float option1 = 0, float option2 = 0)
        {
            this.wait = wait;
            this.type = noteType;
            this.x = x;
            this.length = length;
            this.option1 = option1;
            this.option2 = option2;
        }
    }

    public abstract class NoteCreateBase<T> : CommandBase where T : struct, INoteData
    {
        [SerializeField] protected Mirror mirror;
        [SerializeField] float speedRate = 1f;

        /// <summary>
        /// NoteData内のWaitの加算(秒単位)
        /// </summary>
        protected float WaitDelta { get; private set; }
        protected override float Speed => base.Speed * speedRate;

        protected abstract T[] NoteDatas { get; }
        protected abstract UniTask MoveAsync(RegularNote note, T data);
        protected virtual void OnExecute() { }
        protected virtual float MoveTime => Helper.GetTimeInterval(4, 6);

        protected override async UniTask ExecuteAsync()
        {
            WaitDelta = 0;
            OnExecute();
            foreach (var data in NoteDatas)
            {
                await Wait(data.Wait);
                WaitDelta += Helper.GetTimeInterval(data.Wait);
                CreateNote(data);
            }
        }

        void CreateNote(in T noteData)
        {
            var type = noteData.Type;
            if (type is RegularNoteType.Normal or RegularNoteType.Slide or RegularNoteType.Flick)
            {
                RegularNote note = Helper.GetRegularNote(type);
                MoveAsync(note, noteData).Forget();
            }
            else if (type is RegularNoteType.Hold)
            {
                if (noteData.Length == 0)
                {
                    Debug.LogWarning("ホールドの長さが0です");
                    return;
                }
                float holdTime = Helper.GetTimeInterval(noteData.Length);
                HoldNote hold = Helper.GetHold(holdTime * Speed);
                Vector3 startPos = mirror.Conv(new Vector3(noteData.X, GetStartBase()));
                hold.SetMaskPos(new Vector2(startPos.x, 0));
                MoveAsync(hold, noteData).Forget();
            }
        }

        /// <summary>
        /// 指定したタイミングで処理ができるWhileYieldAsync
        /// </summary>
        /// <param name="waitDelta">グループ先頭からとの時間の差(秒)</param>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <param name="loopCount">タイミングの数</param>
        /// <param name="loopWait">タイミングの間隔(LPB)</param>
        /// <param name="timingAction"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected async UniTask WhileYieldGroupAsync(
            float time, Action<float> action, int loopCount, float loopWait, Action<(int index, float t, float d)> timingAction, float waitDelta = -1, float delta = -1)
        {
            if (waitDelta == -1)
            {
                waitDelta = WaitDelta;
            }
            int index = 0;
            float next = Helper.GetTimeInterval(loopWait) - waitDelta;
            while (next < 0 && index < loopCount)
            {
                next += Helper.GetTimeInterval(loopWait);
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
                if (t >= next)
                {
                    if (index < loopCount)
                    {
                        float d = t - next;
                        next += Helper.GetTimeInterval(loopWait);
                        timingAction.Invoke((index, t, d));
                        index++;
                    }
                    else
                    {
                        next = float.MaxValue;
                    }
                }
                await Helper.Yield();
            }
            action.Invoke(time);
        }

        /// <summary>
        /// 指定したタイミングで処理ができるWhileYieldAsync
        /// </summary>
        /// <param name="waitDelta">グループ先頭からとの時間の差(秒)</param>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <param name="timings">タイミングの配列</param>
        /// <param name="timingAction"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected async UniTask WhileYieldGroupAsync(
            float time, Action<float> action, float[] timings, Action<(int index, float t, float d)> timingAction, float waitDelta = -1, float delta = -1)
        {
            if (waitDelta == -1)
            {
                waitDelta = WaitDelta;
            }
            float next = Helper.GetTimeInterval(timings[0]) - waitDelta;
            int index = 1;
            while (next < 0 && index < timings.Length)
            {
                next += Helper.GetTimeInterval(timings[index]);
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
                if (t >= next)
                {
                    if (index < timings.Length)
                    {
                        float d = t - next;
                        next += Helper.GetTimeInterval(timings[index]);
                        timingAction.Invoke((index, t, d));
                        index++;
                    }
                    else
                    {
                        next = float.MaxValue;
                    }
                }
                await Helper.Yield();
            }
            action.Invoke(time);
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
            FumenDebugUtility.DebugPreview2DNotes(NoteDatas, Helper, mirror, beforeClear, beatDelta);
        }
#endif
    }
}
