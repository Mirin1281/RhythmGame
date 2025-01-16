using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇ノーツ点滅"), System.Serializable]
    public class F_NotesBlink : CommandBase
    {
        [Flags]
        enum BlinkTargets
        {
            Normal = 1 << 0,
            Slide = 1 << 1,
            Flick = 1 << 2,
            Hold = 1 << 3,
            Sky = 1 << 4,
            Arc = 1 << 5,
            Line = 1 << 6,
        }

        [SerializeField] BlinkTargets target = BlinkTargets.Normal | BlinkTargets.Slide | BlinkTargets.Flick | BlinkTargets.Hold;
        [SerializeField, Min(0)] float delay;
        [SerializeField] int blinkCount = 20;
        [SerializeField] int seed = 222;
        [SerializeField] Vector2Int hideWaitRange = new Vector2Int(1, 5);
        [SerializeField] Vector2Int showWaitRange = new Vector2Int(1, 3);
        [SerializeField] bool isDelayOneFrame = true;

        protected override async UniTask ExecuteAsync()
        {
            if (target == 0) return;
            if (delay > 0)
            {
                await Helper.WaitSeconds(delay + Delta);
            }
            await WaitOnTiming();

            List<ItemBase> items = new(100);
            if (target.HasFlag(BlinkTargets.Normal))
            {
                items.AddRange(Helper.PoolManager.RegularPool.GetInstances(0));
            }
            if (target.HasFlag(BlinkTargets.Hold))
            {
                items.AddRange(Helper.PoolManager.HoldPool.GetInstances(0));
            }
            if (target.HasFlag(BlinkTargets.Arc))
            {
                items.AddRange(Helper.PoolManager.ArcPool.GetInstances(0));
            }

            if (target.HasFlag(BlinkTargets.Line))
            {
                items.AddRange(Helper.PoolManager.LinePool.GetInstances(0));
            }

            if (isDelayOneFrame)
            {
                await UniTask.DelayFrame(1, cancellationToken: Helper.Token);
            }

            var rand = new System.Random(seed);
            float interval = 1 / 120f;
            for (int i = 0; i < blinkCount; i++)
            {
                await Helper.WaitSeconds(interval * rand.Next(hideWaitRange.x, hideWaitRange.y));
                SetRendererEnableds(items, false);
                await Helper.WaitSeconds(interval * rand.Next(showWaitRange.x, showWaitRange.y));
                SetRendererEnableds(items, true);
            }
        }

        void SetRendererEnableds(IEnumerable<ItemBase> notes, bool enabled)
        {
            foreach (var note in notes)
            {
                note.SetRendererEnabled(enabled);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
