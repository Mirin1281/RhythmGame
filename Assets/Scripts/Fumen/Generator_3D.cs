using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    public abstract class Generator_3D : NoteGeneratorBase
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse => isInverse;
        protected void SetInverse(bool inverse) => isInverse = inverse;

        /// <summary>
        /// 反転に対応した値にします
        /// </summary>
        protected float GetInverse(float x) => x * (isInverse ? -1 : 1);

        /// <summary>
        /// この値を大きくするとWaitの待機ループが
        /// 生成速度に追いつかなくなる現象が改善されます
        /// </summary>
        const float intervalRange = 0.008f;

        protected virtual float Speed => RhythmGameManager.Speed3D;

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

        protected UniTask WaitSeconds(float time) => UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: Helper.Token);

        protected async UniTask<float> Wait(float lpb, int num = 1)
        {
            if(lpb == 0) return Delta;
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

        protected async UniTask<float> SkyLoop(float lpb, params Vector2?[] nullablePoses)
        {
            foreach(var nullablePos in nullablePoses)
            {
                if(nullablePos is Vector2 pos)
                {
                    Sky(pos, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }

        protected ArcNote Arc(ArcCreateData[] datas, ArcColorType colorType, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, Speed, IsInverse).Forget();
            arc.SetColor(colorType, IsInverse);
            var startPos = new Vector3(0, 0f, StartBase);
            DropAsync(arc, startPos, delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected SkyNote Sky(Vector2 pos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            SkyNote sky = Helper.SkyNotePool.GetNote();
            var startPos = new Vector3(GetInverse(pos.x), pos.y, StartBase);
            DropAsync(sky, startPos, delta).Forget();

            float distance = startPos.z - From - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            var expect = new NoteExpect(sky, sky.transform.position, expectTime);
            Helper.NoteInput.AddExpect(expect);
            return sky;
        }

        protected async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = new Vector3(0, 0, -Speed);
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