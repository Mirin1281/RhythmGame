using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/2_1 回転ホールド"), System.Serializable]
    public class F_Lyrith2_1 : Generator_Type1
    {
        [SerializeField] Vector2 toPos = new Vector2(0, -4);

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            MoveHold(toPos, 2);
        }

        void MoveHold(Vector2 toPos, float length)
        {
            var hold = Helper.HoldNotePool.GetNote();
            var holdTime = 240f / Helper.Metronome.Bpm / length;
            hold.SetLength(holdTime * Speed);
            hold.SetMaskLocalPos(new Vector2(GetInverse(toPos.x), toPos.y));
            hold.SetMaskLength(10);
            var startPos = new Vector2(GetInverse(toPos.x), StartBase + toPos.y);

            float distance = startPos.y - toPos.y - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            float duringTime = holdTime;
            float holdEndTime = duringTime + expectTime;
            var expect = new NoteExpect(hold, new Vector2(startPos.x, toPos.y), expectTime, holdEndTime);
            Helper.NoteInput.AddExpect(expect);

            MoveRotateAsync(hold, startPos, toPos, expectTime, duringTime).Forget();


            async UniTask MoveRotateAsync(HoldNote hold, Vector2 startPos, Vector2 toPos, float expectTime, float holdingTime)
            {
                float baseTime = CurrentTime - Delta;
                var vec = Speed * Vector2.down;
                float time;
                while (hold.IsActive && CurrentTime < expectTime)
                {
                    time = CurrentTime - baseTime;
                    hold.SetPos(startPos + time * vec);
                    hold.SetMaskLocalPos(new Vector2(startPos.x + time * vec.x, toPos.y));
                    await UniTask.Yield(Helper.Token);
                }

                float delta = CurrentTime - expectTime;
                baseTime = CurrentTime - delta;
                time = 0f;
                while (hold.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    var deg = time.Ease(0, 360f, holdingTime, EaseType.OutQuart);
                    vec = Speed * new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad));
                    hold.SetPos(toPos + time * vec);
                    hold.SetRotate(deg);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }
    }
}