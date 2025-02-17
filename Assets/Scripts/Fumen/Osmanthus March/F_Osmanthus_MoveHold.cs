using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Osmanthus/移動ホールド"), System.Serializable]
    public class F_Osmanthus_MoveHold : CommandBase
    {
        [Serializable]
        struct MoveHoldData
        {
            [field: SerializeField] public Lpb Wait { get; private set; }
            [field: SerializeField] public float FromX { get; private set; }
            [field: SerializeField] public Lpb Length { get; private set; }
        }

        [SerializeField] Mirror mirror;

        [SerializeField] MoveHoldData[] datas = new MoveHoldData[1];

        protected override async UniTaskVoid ExecuteAsync()
        {
            for (int i = 0; i < datas.Length; i++)
            {
                var d = datas[i];
                await Wait(d.Wait);
                MoveHold(d.Length, d.FromX);
            }

            await UniTask.CompletedTask;
        }

        void MoveHold(Lpb length, float fromX)
        {
            float startX = fromX > 0 ? fromX + 3 : fromX - 3;
            MoveHold(length, startX, fromX, EaseType.InQuad);
        }

        void MoveHold(Lpb length, float startX, float fromX, EaseType easeType = EaseType.Linear)
        {
            var hold = Hold(startX, length);
            MoveAsync(hold).Forget();


            async UniTaskVoid MoveAsync(HoldNote hold)
            {
                float baseTime = CurrentTime - Delta;
                var startPos = hold.GetPos();
                Easing easing = new Easing(startX, fromX, MoveLpb.Time, easeType);

                float time = 0f;
                while (hold.IsActive && time < 8f)
                {
                    time = CurrentTime - baseTime;
                    var posX = mirror.Conv(easing.Ease(time));
                    var posY = startPos.y + time * -Speed;
                    hold.SetPos(new Vector3(posX, posY));
                    hold.SetMaskPos(new Vector2(posX, 0));
                    await Yield();
                }
            }
        }

        HoldNote Hold(float x, Lpb length)
        {
            HoldNote hold = Helper.GetHold(length * Speed);
            Vector3 startPos = mirror.Conv(new Vector3(x, StartBase));
            hold.SetMaskPos(new Vector2(startPos.x, 0));
            hold.SetPos(startPos);

            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, length);
            return hold;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "MoveHold";
        }
#endif
    }
}