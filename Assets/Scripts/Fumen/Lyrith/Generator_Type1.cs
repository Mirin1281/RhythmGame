using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    public abstract class Generator_Type1 : NoteGeneratorBase
    {
        [SerializeField] bool isInverse;

        /// <summary>
        /// 反転に対応した値にします
        /// </summary>
        protected float GetInverse(float x) => x * (isInverse ? -1 : 1);

        /// <summary>
        /// この値を大きくするとWaitの待機ループが
        /// 生成速度に追いつかなくなる現象が改善されます
        /// </summary>
        const float intervalRange = 0.008f;

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + From + 0.2f;
        protected virtual float From => -4f;
        protected float CurrentTime => Helper.Metronome.CurrentTime;         

        float GetInterval(float lpb, int num = 1)
        {
            if(lpb == 0) return 0;
            return 240f / Helper.Metronome.Bpm / lpb * num;
        }
        public async UniTask<float> Wait(float lpb, int num = 1)
        {
            if(lpb == 0) return 0;
            float baseTime = CurrentTime;
            float interval = GetInterval(lpb, num);
            while (true)
            {
                float currentTime = CurrentTime - baseTime;
                if (currentTime >= interval - intervalRange)
                {
                    Delta = currentTime - interval + Delta;
                    return Delta;
                }
                await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, Helper.Token);
            }
        }
        public async UniTask<float> WaitPlane(float lpb, float delta, int num = 1)
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
        }

        protected async UniTask<float> Loop(float lpb, NoteType type, params float?[] xs)
        {
            for(int i = 0; i < xs.Length; i++)
            {
                if(xs[i] is float x)
                {
                    Note(x, type, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }
        protected async UniTask<float> LoopPlane(float lpb, NoteType type, float delta, params float?[] xs)
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
        }

        public Generator_Type1 Note(float x, NoteType type, float delta = -1)
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
            var startPos = new Vector3(GetInverse(x), StartBase);
            DropAsync(note, startPos, delta).Forget();

            float distance = startPos.y - From - Speed * delta;
            float expectTime = distance / Speed + CurrentTime;
            var expect = new NoteExpect(note, new Vector2(startPos.x, From), expectTime);
            Helper.NoteInput.AddExpect(expect);
            return this;
        }
        public Generator_Type1 Double(float x1, float x2, NoteType type = NoteType.Normal, NoteType type2 = NoteType._None, float delta = -1)
        {
            if(type2 == NoteType._None)
            {
                type2 = type;
            }
            Note(x1, type, delta).Note(x2, type2, delta);
            return this;
        }
        public Generator_Type1 Hold(float x, float length, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            var hold = Helper.HoldNotePool.GetNote();
            var holdTime = GetInterval(length);
            hold.SetLength(holdTime * Speed);
            hold.SetMaskLocalPos(new Vector2(GetInverse(x), From));
            var startPos = new Vector3(GetInverse(x), StartBase);
            DropAsync(hold, startPos, delta).Forget();

            float distance = startPos.y - From - Speed * delta;
            float expectTime = distance / Speed + CurrentTime;
            float holdEndTime = holdTime + expectTime;
            var expect = new NoteExpect(hold, new Vector2(startPos.x, From), expectTime, holdEndTime);
            Helper.NoteInput.AddExpect(expect);
            return this;
        }

        public void WhileYield(float time, Action<float> action)
            => WhileYieldAsync(time, action).Forget();
        public async UniTask WhileYieldAsync(float time, Action<float> action)
        {
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
        public void WhileYieldPlane(float time, float delta, Action<float> action)
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
        }

        protected async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed * Vector3.down;
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