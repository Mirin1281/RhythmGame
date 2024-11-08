using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [Serializable]
    public struct HoldData
    {
        [SerializeField] bool isDiabled;
        [SerializeField] float x;
        [SerializeField, Min(0)] float wait;
        [SerializeField, Min(0)] float width;
        [SerializeField, Min(0)] float length;

        public readonly bool IsDisabled => isDiabled;
        public readonly float X => x;
        public readonly float Wait => wait;
        public readonly float Width => width;
        public readonly float Length => length;
    }

    [AddTypeMenu("AprilRabbit/タッチのホールド"), System.Serializable]
    public class F_AprilRabbit_TouchHold : Generator_Common
    {
        /*[SerializeField] float speedRate = 1f;

        [SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = true;*/
        
        [SerializeField] HoldData[] noteDatas = new HoldData[1];

        //protected override float Speed => base.Speed * speedRate;

        protected override async UniTask GenerateAsync()
        {
            foreach(var data in noteDatas)
            {
                if(data.IsDisabled == false)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    TouchHold(data.X, data.Length);
                }
                await Wait(data.Wait);
            }
        }


        HoldNote TouchHold(float x, float length, float delta = -1, bool isSpeedChangable = false, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
            Vector3 startPos = new (Inv(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            if(isSpeedChangable)
            {
                DropAsync_SpeedChangable(hold, delta).Forget();
            }
            else
            {
                DropAsync(hold, startPos, delta).Forget();
            }

            float expectTime = startPos.y / Speed - delta;
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(hold, expectTime, 0);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, 0, mode: NoteExpect.ExpectMode.Y_Static);
            }
            return hold;


            async UniTask DropAsync_SpeedChangable(HoldNote hold, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                while (hold.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    var vec = Speed * Vector3.down;
                    hold.SetLength(holdTime * Speed);
                    hold.SetPos(new Vector3(Inv(x), StartBase, -0.04f) + time * vec);
                    await Helper.Yield();
                }
            }
        }

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
            return noteDatas.Length + GetInverseSummary();
        }
    }
}
