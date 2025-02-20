using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇アイテム振動"), System.Serializable]
    public class F_ItemVibrate : CommandBase
    {
        [Space(20)]
        [SerializeField] ItemTargets target = ItemTargets.Normal | ItemTargets.Slide | ItemTargets.Hold;
        [SerializeField] int capacity = 100;
        [SerializeField] Lpb time = new Lpb(4);

        [Space(20)]
        [SerializeField] float startIntensity = 0.5f;
        [SerializeField] float endIntensity = 0f;
        [SerializeField] EaseType easeType = EaseType.OutCubic;

        [Space(20)]
        [SerializeField] float frequency = 100;

        [SerializeField, Tooltip("-1に設定すると乱数を使用しません")]
        int randomSeed = 222;

        [SerializeField, Tooltip("アイテムの取得後、1フレーム待ってから振動を開始します\n処理時間のスパイクを緩和することができます")]
        bool isDelayOneFrame = true;

        protected override async UniTaskVoid ExecuteAsync()
        {
            if (target == 0) return;
            await WaitOnTiming();
            await Yield(); // 同カウントだと取り逃がすことがあるので1フレーム待つ

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
            if (target.HasFlag(ItemTargets.Arc))
            {
                items.AddRange(Helper.PoolManager.ArcPool.GetInstances());
            }

            if (target.HasFlag(ItemTargets.Line))
            {
                items.AddRange(Helper.PoolManager.LinePool.GetInstances());
            }

            if (isDelayOneFrame)
            {
                await Yield();
            }

            var rand = randomSeed == -1 ? null : new System.Random(randomSeed);
            var ampEasing = new Easing(startIntensity, endIntensity, time.Time, easeType);
            foreach (var item in items)
            {
                Vibrate(item, ampEasing, rand);
            }
        }

        void Vibrate(ItemBase item, Easing ampEasing, System.Random rand)
        {
            float baseX = item.GetPos().x;
            if (rand == null)
            {
                WhileYield(time.Time, t =>
                {
                    float x = baseX + Mathf.Sin(t * frequency) * ampEasing.Ease(t);
                    item.SetPos(new Vector3(x, item.GetPos().y));
                }, Delta);
            }
            else
            {
                WhileYield(time.Time, t =>
                {
                    float x = baseX + Mathf.Cos(t * frequency) * ampEasing.Ease(t) * rand.GetFloat(-1, 1);
                    item.SetPos(new Vector3(x, item.GetPos().y));
                }, Delta);
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }
#endif
    }
}
