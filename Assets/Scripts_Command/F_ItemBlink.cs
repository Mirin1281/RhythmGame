using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace NoteCreating
{
    [Flags]
    public enum ItemTargets
    {
        Normal = 1 << 0,
        Slide = 1 << 1,
        Hold = 1 << 3,
        Circle = 1 << 4,
        Arc = 1 << 5,
        Line = 1 << 6,
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◇アイテム 点滅"), System.Serializable]
    public class F_ItemBlink : CommandBase
    {
        [Space(20)]
        [SerializeField] ItemTargets target = ItemTargets.Normal | ItemTargets.Slide | ItemTargets.Hold;
        [SerializeField] int capacity = 16;
        [Space(10)]
        [SerializeField] int blinkCount = 16;
        [SerializeField] Lpb interval = new Lpb(32);
        [Space(10)]
        [SerializeField] bool useRandom = true;
        [SerializeField] int seed = 222;
        [SerializeField] Vector2Int hideWaitRange = new Vector2Int(1, 5);
        [SerializeField] Vector2Int showWaitRange = new Vector2Int(1, 3);


        [SerializeField, Tooltip("アイテムの取得後、1フレーム待ってから振動を開始します\n処理時間のスパイクを緩和することができます")]
        bool isDelayOneFrame = true;

        protected override async UniTaskVoid ExecuteAsync()
        {
            if (target == 0) return;
            await Wait(MoveLpb);

            List<ItemBase> items = new(capacity);
            if (target.HasFlag(ItemTargets.Normal))
            {
                items.AddRange(Helper.PoolManager.RegularPool.GetInstances(0));
            }
            if (target.HasFlag(ItemTargets.Slide))
            {
                items.AddRange(Helper.PoolManager.RegularPool.GetInstances(1));
            }
            if (target.HasFlag(ItemTargets.Hold))
            {
                items.AddRange(Helper.PoolManager.HoldPool.GetInstances());
            }
            if (target.HasFlag(ItemTargets.Circle))
            {
                items.AddRange(Helper.PoolManager.CirclePool.GetInstances());
            }
            if (target.HasFlag(ItemTargets.Arc))
            {
                items.AddRange(Helper.PoolManager.ArcPool.GetInstances());
            }
            if (target.HasFlag(ItemTargets.Line))
            {
                items.AddRange(Helper.PoolManager.LinePool.GetInstances());
            }

#if UNITY_EDITOR
            if (items.Count > capacity)
            {
                Debug.LogWarning($"<color=red>アイテム数: {items.Count}, capacity: {capacity}</color>");
            }
            else
            {
                Debug.Log($"アイテム数: {items.Count}, capacity: {capacity}");
            }
#endif

            if (isDelayOneFrame)
            {
                await UniTask.DelayFrame(1, cancellationToken: Helper.Token);
            }

            if (useRandom)
            {
                var rand = new System.Random(seed);
                float interval = 1 / 120f;
                for (int i = 0; i < blinkCount; i++)
                {
                    await WaitSeconds(interval * rand.Next(hideWaitRange.x, hideWaitRange.y));
                    SetRendererEnableds(items, false);
                    await WaitSeconds(interval * rand.Next(showWaitRange.x, showWaitRange.y));
                    SetRendererEnableds(items, true);
                }
            }
            else
            {
                for (int i = 0; i < blinkCount; i++)
                {
                    SetRendererEnableds(items, false);
                    await Wait(interval);
                    SetRendererEnableds(items, true);
                    await Wait(interval);
                }
            }
        }

        void SetRendererEnableds(IEnumerable<ItemBase> notes, bool enabled)
        {
            foreach (var note in notes)
            {
                note.SetRendererEnabled(enabled);
            }
        }

#if UNITY_EDITOR
        protected override string GetSummary()
        {
            return target.ToString();
        }

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }
#endif
    }
}
