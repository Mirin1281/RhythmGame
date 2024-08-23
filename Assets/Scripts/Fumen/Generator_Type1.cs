using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    public abstract class Generator_Type1 : NoteGeneratorBase
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse => isInverse;
        protected void SetInverse(bool inverse) => isInverse = inverse;

        /// <summary>
        /// 反転に対応した値にします
        /// </summary>
        protected float GetInverse(float x) => x * (isInverse ? -1 : 1);

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + From + 0.2f;
        protected float From => 0f;
        protected float CurrentTime => Helper.Metronome.CurrentTime;         

        protected float GetTimeInterval(float lpb, int num = 1)
        {
            if(lpb == 0) return 0;
            return 240f / Helper.Metronome.Bpm / lpb * num;
        }

        /// <summary>
        /// この値を大きくするとWaitの待機ループが
        /// 生成速度に追いつかなくなる現象が改善されます
        /// </summary>
        const float intervalRange = 0.008f;

        protected UniTask WaitSeconds(float time) => MyUtility.WaitSeconds(time, Helper.Token);

        protected async UniTask<float> Wait(float lpb, int num = 1)
        {
            if(lpb == 0 || num == 0) return Delta;
            float baseTime = CurrentTime;
            float interval = GetTimeInterval(lpb, num);
            await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval - intervalRange, cancellationToken: Helper.Token);
            Delta += CurrentTime - baseTime - interval;
            return Delta;
        }

        protected void WhileYield(float time, Action<float> action, float delta = -1)
            => WhileYieldAsync(time, action, delta).Forget();
        protected async UniTask WhileYieldAsync(float time, Action<float> action, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            if(time == 0)
            {
                action?.Invoke(time);
                return;
            }
            float baseTime = CurrentTime - Delta;
            float t = 0f;
            while(t < time)
            {
                t = CurrentTime - baseTime;
                action?.Invoke(t);
                await UniTask.Yield(Helper.Token);
            }
            action?.Invoke(time);
        }
        /*public async UniTask<float> WaitPlane(float lpb, float delta, int num = 1)
        {
            if(lpb == 0) return 0;
            float baseTime = CurrentTime;
            float interval = GetInterval(lpb, num);
            while (true)
            {
                float currentTime = CurrentTime - baseTime;
                if (currentTime >= interval - intervalRange)
                {
                    return currentTime - interval + delta;
                }
                await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, Helper.Token);
            }
        }*/

        protected async UniTask<float> Loop(float lpb, NoteType type, params float?[] nullableXs)
        {
            foreach(var nullableX in nullableXs)
            {
                if(nullableX is float x)
                {
                    Note(x, type, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }
        /*protected async UniTask<float> LoopPlane(float lpb, NoteType type, float delta, params float?[] xs)
        {
            for(int i = 0; i < xs.Length; i++)
            {
                if(xs[i] is float x)
                {
                    Note(x, type, delta);
                }
                delta = await Wait(lpb);
            }
            return delta;
        }*/

        protected NoteBase Note(float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase note = type switch
            {
                NoteType.Normal => Helper.NormalNotePool.GetNote(),
                NoteType.Slide => Helper.SlideNotePool.GetNote(),
                NoteType.Flick => Helper.FlickNotePool.GetNote(),
                _ => throw new ArgumentOutOfRangeException()
            };
            Vector3 startPos = new Vector3(GetInverse(x), StartBase);
            DropAsync(note, startPos, delta).Forget();

            float distance = startPos.y - From - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, new Vector2(startPos.x, From), expectTime);
            Helper.NoteInput.AddExpect(expect);
            return note;
        }
        protected HoldNote Hold(float x, float length, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            HoldNote hold = Helper.HoldNotePool.GetNote();
            float holdTime = GetTimeInterval(length);
            hold.SetLength(holdTime * Speed);
            Vector3 startPos = new Vector3(GetInverse(x), StartBase);
            hold.SetMaskLocalPos(new Vector2(startPos.x, From));
            DropAsync(hold, startPos, delta).Forget();

            float distance = startPos.y - From - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            float holdEndTime = expectTime + holdTime;
            NoteExpect expect = new NoteExpect(hold, new Vector2(startPos.x, From), expectTime, holdEndTime);
            Helper.NoteInput.AddExpect(expect);
            return hold;
        }

        
        /*public void WhileYieldPlane(float time, float delta, Action<float> action)
            => WhileYieldAsyncPlane(time, delta, action).Forget();
        public async UniTask WhileYieldAsyncPlane(float time, float delta, Action<float> action)
        {
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while(t < time)
            {
                t = CurrentTime - baseTime;
                action?.Invoke(t);
                await UniTask.Yield(Helper.Token);
            }
            action?.Invoke(time);
        }*/

        protected async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = new Vector3(0, -Speed, 0);
            while (note.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await UniTask.Yield(Helper.Token);
            }
        }

        protected string GetInverseSummary()
        {
            if(isInverse)
            {
                return " <color=#0000ff><b>(inv)</b></color>";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}