using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/律動するホールド"), System.Serializable]
    public class F_AprilRabbit_RhythmHold : CommandBase
    {
        [SerializeField] Mirror _mirror;
        [SerializeField] float _x = 0;
        [SerializeField] float _length = 0.5f;
        [SerializeField] float _shakeX = 0.5f;
        [SerializeField] float _shakeInterval = 2f;
        [SerializeField] float _shakeCount = 4f;

        protected override async UniTask ExecuteAsync()
        {
            var hold = Hold(_x, _length);
            await WaitOnTiming();
            for (int i = 0; i < _shakeCount; i++)
            {
                MoveAsync(hold, _mirror.Conv(i % 2 == 0 ? _shakeX : -_shakeX)).Forget();
                await Wait(_shakeInterval);
            }


            async UniTaskVoid MoveAsync(HoldNote hold, float shakeX)
            {
                float time = Helper.GetTimeInterval(_shakeInterval);
                float baseTime = CurrentTime - Delta;
                var startPos = hold.GetPos();
                Easing easing = new Easing(startPos.x + shakeX, startPos.x, time, EaseType.OutCubic);

                float t = 0f;
                while (hold.IsActive && t < time)
                {
                    t = CurrentTime - baseTime;
                    hold.SetPos(new Vector3(easing.Ease(t), hold.GetPos().y));
                    hold.SetMaskPos(new Vector3(easing.Ease(t), 0));
                    await Helper.Yield(timing: PlayerLoopTiming.PreLateUpdate);
                }
            }
            await UniTask.CompletedTask;
        }


        HoldNote Hold(float x, float length)
        {
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed);
            Vector3 startPos = _mirror.Conv(new Vector3(x, GetStartBase()));
            hold.SetMaskPos(new Vector2(startPos.x, 0));
            hold.SetPos(startPos);

            DropAsync(hold, startPos).Forget();

            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, holdTime);

            return hold;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "RhythmHold";
        }
#endif
    }
}