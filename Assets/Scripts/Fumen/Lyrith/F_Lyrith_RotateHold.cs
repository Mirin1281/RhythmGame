using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("Lyrith/2_1 回転ホールド"), System.Serializable]
    public class F_Lyrith_RotateHold : CommandBase
    {
        [SerializeField] Mirror mirror;
        protected override async UniTask ExecuteAsync()
        {
            await UniTask.CompletedTask;
            MoveHold(-2, 4, true);
            MoveHold(2, 4, false);
        }

        void MoveHold(float x, float length, bool left)
        {
            var holdTime = Helper.GetTimeInterval(length);
            var hold = Helper.GetHold(holdTime * Speed);
            var startPos = new Vector2(mirror.Conv(x), StartBase);
            var toPos = new Vector2(mirror.Conv(x), 0);
            hold.SetMaskLocalPos(toPos);
            hold.SetMaskLength(10);

            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, holdTime);

            MoveRotateAsync(hold, startPos, toPos, expectTime, holdTime, left).Forget();


            async UniTask MoveRotateAsync(HoldNote hold, Vector2 startPos, Vector2 toPos, float expectTime, float holdingTime, bool left)
            {
                float baseTime = CurrentTime - Delta;
                var vec = Speed * Vector2.down;
                float time = 0;
                while (hold.IsActive && time < expectTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetPos(startPos + time * vec);
                    hold.SetMaskLocalPos(new Vector2(startPos.x + time * vec.x, 0));
                    await Helper.Yield();
                }

                // ここから着地後
                baseTime = CurrentTime + expectTime;
                time = 0f;
                while (hold.IsActive && time < 3f)
                {
                    time = CurrentTime + expectTime - baseTime;
                    float deg = time.Ease(0, 360f, holdingTime, EaseType.OutQuad) * (left ? 1 : -1);
                    vec = Speed * new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad));
                    hold.SetPos(toPos + time * vec);
                    hold.SetRot(deg);
                    await Helper.Yield();
                }
            }
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "RotateHold";
        }
#endif
    }
}