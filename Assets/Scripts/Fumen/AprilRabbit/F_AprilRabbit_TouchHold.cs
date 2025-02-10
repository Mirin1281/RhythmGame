using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/タッチのホールド")]
    public class F_AprilRabbit_TouchHold : CommandBase
    {
        [Serializable]
        public struct HoldData
        {
            [SerializeField] bool isDiabled;
            [SerializeField, Min(0)] Lpb wait;
            [SerializeField, Space(10)] float x;
            [SerializeField, Min(0)] Lpb length;

            public readonly bool IsDisabled => isDiabled;
            public readonly Lpb Wait => wait;
            public readonly float X => x;
            public readonly Lpb Length => length;
        }
        /*[SerializeField] float speedRate = 1f;

        [SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = true;*/

        [SerializeField] Mirror mirror;
        [SerializeField] HoldData[] noteDatas = new HoldData[1];

        //protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            foreach (var data in noteDatas)
            {
                await Wait(data.Wait);
                if (data.IsDisabled) continue;
                if (data.Length == default)
                {
                    Debug.LogWarning("ホールドの長さが0です");
                    continue;
                }
                TouchHold(data.X, data.Length);
            }
        }


        HoldNote TouchHold(float x, Lpb length, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            HoldNote hold = Helper.GetHold(length * Speed);
            Vector3 startPos = new(mirror.Conv(x), StartBase, -0.04f);
            hold.SetMaskPos(new Vector2(startPos.x, 0));
            DropAsync(hold, startPos.x, delta).Forget();

            float expectTime = startPos.y / Speed - delta;
            Helper.NoteInput.AddExpect(hold, expectTime, new Lpb(0));
            return hold;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "TouchHold";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(
                255,
                (byte)Mathf.Clamp(246 - noteDatas.Length * 2, 96, 246),
                (byte)Mathf.Clamp(230 - noteDatas.Length * 2, 130, 230),
                255);
        }

        protected override string GetSummary()
        {
            return noteDatas.Length + mirror.GetStatusText();
        }
#endif
    }
}
