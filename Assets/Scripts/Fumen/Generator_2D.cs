using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    public abstract class Generator_2D : NoteGeneratorBase
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse => isInverse;
        protected void SetInverse(bool inverse) => isInverse = inverse;

        /// <summary>
        /// 反転に対応した値にします
        /// </summary>
        protected float Inverse(float x) => x * (isInverse ? -1 : 1);

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + 0.2f;
        protected float CurrentTime => Helper.Metronome.CurrentTime;

        protected UniTask WaitSeconds(float time) => MyUtility.WaitSeconds(time, Helper.Token);

        protected async UniTask<float> Wait(float lpb, int num = 1)
        {
            if(lpb == 0 || num == 0) return Delta;
            float baseTime = CurrentTime;
            float interval = Helper.GetTimeInterval(lpb, num);
            if(Delta > interval)
            {
                Delta -= interval;
            }
            else
            {
                await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval, cancellationToken: Helper.Token);
                Delta += CurrentTime - baseTime - interval;
            }
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
                action.Invoke(time);
                return;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while(t < time)
            {
                t = CurrentTime - baseTime;
                action.Invoke(t);
                await UniTask.Yield(Helper.Token);
            }
            action.Invoke(time);
        }

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

        protected NoteBase_2D Note(float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            Vector3 startPos = new Vector3(Inverse(x), StartBase);
            DropAsync(note, startPos, delta).Forget();

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, new Vector2(startPos.x, 0), expectTime);
            Helper.NoteInput.AddExpect(expect);
            
            return note;
        }
        protected HoldNote Hold(float x, float length, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            HoldNote hold = Helper.GetHold();
            float holdTime = Helper.GetTimeInterval(length);
            hold.SetLength(holdTime * Speed);
            Vector3 startPos = new Vector3(Inverse(x), StartBase);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            DropAsync(hold, startPos, delta).Forget();

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            float holdEndTime = expectTime + holdTime;
            NoteExpect expect = new NoteExpect(hold, new Vector2(startPos.x, 0), expectTime, holdEndTime);
            Helper.NoteInput.AddExpect(expect);
            return hold;
        }

        protected async UniTask DropAsync(NoteBase_2D note, Vector3 startPos, float delta = -1)
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