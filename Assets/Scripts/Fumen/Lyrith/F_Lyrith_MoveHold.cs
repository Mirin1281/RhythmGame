using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/移動ホールド"), System.Serializable]
    public class F_Lyrith_MoveHold : Generator_Type1
    {
        float MoveX => 0.15f;

        protected override async UniTask GenerateAsync()
        {
            MoveHold(-9, 4, MoveX);
            await Loop(8, NoteType.Normal,
                3,
                5,
                3
            );

            MoveHold(9, 4, -MoveX);
            await Loop(8, NoteType.Normal,
                -3,
                -5,
                -3
            );

            MoveHold(-11, 4, MoveX);
            await Loop(8, NoteType.Normal,
                1,
                3,
                5
            );

            MoveHold(11, 4, -MoveX);
            await Loop(8, NoteType.Normal,
                -1,
                -3,
                -5
            );

            await Loop(16, NoteType.Normal,
                0.5f,
                -0.5f,
                0.5f,
                -0.5f
            );
        }

        void MoveHold(float x, float length, float moveX)
        {
            var hold = Helper.HoldNotePool.GetNote();
            var holdTime = Helper.GetTimeInterval(length);
            hold.SetLength(holdTime * Speed);
            hold.SetMaskLocalPos(new Vector2(Inverse(x), 0));
            var startPos = new Vector3(Inverse(x), StartBase);
            MoveAsync(hold, startPos, Inverse(moveX)).Forget();

            float distance = startPos.y - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            float holdEndTime = holdTime + expectTime;
            var expect = new NoteExpect(hold, new Vector2(startPos.x + distance * Inverse(moveX), 0), expectTime, holdEndTime);
            Helper.NoteInput.AddExpect(expect);
        }

        async UniTask MoveAsync(HoldNote hold, Vector3 startPos, float moveX)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0f;
            var vec = Speed * new Vector3(moveX, -1f);
            while (hold.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                hold.SetPos(startPos + time * vec);
                hold.SetMaskLocalPos(new Vector2(startPos.x + time * vec.x, 0));
                await UniTask.Yield(Helper.Token);
            }
        }
    }
}