using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/律動するホールド"), System.Serializable]
    public class F_AprilRabbit_RhythmHold : Command_General
    {
        [SerializeField] float _x = 0;
        [SerializeField] float _length = 0.5f;
        [SerializeField] float _shakeX = 0.5f;
        [SerializeField] float _shakeInterval = 2f;
        [SerializeField] float _shakeCount = 4f;

        protected override async UniTask ExecuteAsync()
        {
            RhythmHold(_x, _length).Forget();
            await UniTask.CompletedTask;
        }

        async UniTask RhythmHold(float x, float length)
        {
            var hold = Hold(x, length);
            await WaitOnTiming();
            for (int i = 0; i < _shakeCount; i++)
            {
                MoveAsync(
                    hold,
                    Inv(i % 2 == 0 ? _shakeX : -_shakeX)
                ).Forget();
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
                    hold.SetMaskLocalPos(new Vector3(easing.Ease(t), 0));
                    await Helper.Yield(timing: PlayerLoopTiming.PreLateUpdate);
                }
            }
        }

        protected override string GetName()
        {
            return "RhythmHold";
        }
    }
}