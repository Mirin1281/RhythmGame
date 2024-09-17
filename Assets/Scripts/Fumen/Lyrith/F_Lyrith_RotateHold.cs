using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/2_1 回転ホールド"), System.Serializable]
    public class F_Lyrith_RotateHold : Generator_2D
    {
        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            MoveHold(-2, 4, true);
            MoveHold(2, 4, false);
        }

        void MoveHold(float x, float length, bool left)
        {
            var hold = Helper.GetHold();
            var startPos = new Vector2(Inverse(x), StartBase);
            var toPos = new Vector2(Inverse(x), 0);
            var holdTime = Helper.GetTimeInterval(length);
            hold.SetLength(holdTime * Speed);
            hold.SetMaskLocalPos(toPos);
            hold.SetMaskLength(10);
            

            float distance = startPos.y - Speed * Delta;
            float expectTime = CurrentTime + distance / Speed;
            float holdEndTime = expectTime + holdTime;
            var expect = new NoteExpect(hold, new Vector2(startPos.x, 0), expectTime, holdEndTime: holdEndTime);
            Helper.NoteInput.AddExpect(expect);

            MoveRotateAsync(hold, startPos, toPos, expectTime, holdTime, left).Forget();


            async UniTask MoveRotateAsync(HoldNote hold, Vector2 startPos, Vector2 toPos, float expectTime, float holdingTime, bool left)
            {
                float baseTime = CurrentTime - Delta;
                var vec = Speed * Vector2.down;
                float time;
                while (hold.IsActive && CurrentTime < expectTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetPos(startPos + time * vec);
                    hold.SetMaskLocalPos(new Vector2(startPos.x + time * vec.x, 0));
                    await UniTask.Yield(Helper.Token);
                }

                // ここから着地後
                baseTime = expectTime;
                time = 0f;
                while (hold.IsActive && time < 3f)
                {
                    time = CurrentTime - baseTime;
                    float deg = time.Ease(0, 360f, holdingTime, EaseType.OutQuad) * (left ? 1 : -1);
                    vec = Speed * new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad));
                    hold.SetPos(toPos + time * vec);
                    hold.SetRotate(deg);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        protected override string GetName()
        {
            return "RotateHold";
        }
    }
}