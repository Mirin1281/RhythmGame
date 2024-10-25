using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("Osmanthus/移動ホールド"), System.Serializable]
    public class F_Osmanthus_MoveHold : Generator_Common
    {
        [Serializable]
        struct MoveHoldData
        {
            [field: SerializeField] public float Length { get; private set; }
            [field: SerializeField] public float FromX { get; private set; }
            [field: SerializeField] public float Wait { get; private set; }
        }

        [SerializeField] MoveHoldData[] datas = new MoveHoldData[1];

        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < datas.Length; i++)
            {
                var d = datas[i];
                MoveHold(d.Length, d.FromX);
                await Wait(d.Wait);
            }

            await UniTask.CompletedTask;
        }

        void MoveHold(float length, float fromX)
        {
            float startX = fromX > 0 ? fromX + 3 : fromX - 3;
            MoveHold(length, startX, fromX, EaseType.InQuad);
        }

        void MoveHold(float length, float startX, float fromX, EaseType easeType = EaseType.Linear)
        {
            var hold = Hold(startX, length, isMove: false);
            MoveAsync(hold).Forget();


            async UniTaskVoid MoveAsync(HoldNote hold)
            {
                float baseTime = CurrentTime - Delta;
                var startPos = hold.GetPos();
                Easing easing = new Easing(startX, fromX, StartBase / Speed, easeType);

                float time = 0f;
                while (hold.IsActive && time < 8f)
                {
                    time = CurrentTime - baseTime;
                    var posX = Inv(easing.Ease(time));
                    var posY = startPos.y + time * -Speed;
                    hold.SetPos(new Vector3(posX, posY));
                    hold.SetMaskLocalPos(new Vector2(posX, 0));
                    await Helper.Yield();
                }
            }
        }

        protected override string GetName()
        {
            return "MoveHold";
        }
    }
}