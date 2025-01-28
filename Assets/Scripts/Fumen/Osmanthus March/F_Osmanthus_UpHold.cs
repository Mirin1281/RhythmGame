using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu("Osmanthus/昇るホールド"), System.Serializable]
    public class F_Osmanthus_UpHold : CommandBase
    {
        [Serializable]
        struct MoveHoldData
        {
            [field: SerializeField] public float Wait { get; private set; }
            [field: SerializeField] public float X { get; private set; }
            [field: SerializeField] public float Length { get; private set; }
        }

        [SerializeField] Mirror mirror;

        [SerializeField] MoveHoldData[] datas = new MoveHoldData[1];

        protected override async UniTask ExecuteAsync()
        {
            for (int i = 0; i < datas.Length; i++)
            {
                var d = datas[i];
                await Wait(d.Wait);
                UpHold(d.Length, d.X).Forget();
            }

            await UniTask.CompletedTask;
        }

        async UniTaskVoid UpHold(float length, float x)
        {
            var hold = Hold(x, length);
            hold.IsVerticalRange = true;
            float expectTime = StartBase / Speed;
            float holdTime = Helper.GetTimeInterval(length);

            float baseTime = CurrentTime - Delta;
            Vector3 startPos = new Vector3(mirror.Conv(x), StartBase);
            var vec = Speed * Vector3.down;

            float time = 0f;
            while (hold.IsActive && time < expectTime)
            {
                time = CurrentTime - baseTime;
                hold.SetPos(startPos + time * vec);
                await Helper.Yield();
            }

            baseTime = CurrentTime - Delta;
            time = 0f;
            while (hold.IsActive && time < holdTime)
            {
                time = CurrentTime - baseTime;
                hold.SetMaskLocalPos(new Vector3(mirror.Conv(x), 0) - time * vec);
                hold.SetMaskLength(time * holdTime * Speed);
                await Helper.Yield();
            }
            hold.SetActive(false);
        }

        HoldNote Hold(float x, float length)
        {
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed);
            Vector3 startPos = mirror.Conv(new Vector3(x, StartBase));
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            hold.SetPos(startPos);

            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            return hold;
        }

        // 旧実装
        /*void UpHold(float length, float x)
        {
            var (visualHold, expectTime, holdTime) = VisualHold(x, length, isMove: false);
            VisualMoveAsync(visualHold).Forget();

            var judgeHold = Hold(x, length, isMove: false);
            judgeHold.SetRendererEnabled(false);
            judgeHold.SetWidth(5);
            judgeHold.transform.localRotation = Quaternion.Euler(0, 0, 90);
            judgeHold.SetPos(Vector3.zero);
            judgeHold.SetMaskLocalPos(new Vector3(0, -Inv(x)));
            JudgeMoveAsync(judgeHold).Forget();


            async UniTaskVoid VisualMoveAsync(HoldNote hold)
            {
                float baseTime = CurrentTime - Delta;
                Vector3 startPos = new Vector3(Inv(x), StartBase);
                var vec = Speed * Vector3.down;

                float time = 0f;
                while (hold.IsActive && time < expectTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }

                baseTime = CurrentTime - Delta;
                time = 0f;
                while (hold.IsActive && time < holdTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetMaskLocalPos(new Vector3(Inv(x), 0) - time * vec);
                    hold.SetMaskLength(time * holdTime * Speed);
                    await Helper.Yield();
                }
                hold.SetActive(false);
            }

            async UniTaskVoid JudgeMoveAsync(HoldNote hold)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = Speed * Vector3.down;
                while (hold.IsActive && time < expectTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetPos(new Vector3(0, StartBase - Inv(x)) + time * vec);
                    await Helper.Yield();
                }
                hold.SetActive(false);
            }
        }

        (HoldNote, float, float) VisualHold(float x, float length, float delta = -1, bool isMove = true, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
            Vector3 startPos = new (Inv(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            if(isMove)
            {
                DropAsync(hold, startPos, delta).Forget();
            }
            else
            {
                hold.SetPos(startPos);
            }
            return (hold, startPos.y / Speed, holdTime);
        }*/

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "UpHold";
        }
#endif
    }
}