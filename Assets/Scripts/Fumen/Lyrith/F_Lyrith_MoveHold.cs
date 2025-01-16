using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("Lyrith/移動ホールド"), System.Serializable]
    public class F_Lyrith_MoveHold : Command_General
    {
        protected override async UniTask ExecuteAsync()
        {
            float moveX = 2.4f / Speed;

            MoveHold(-9, 4, moveX);
            await LoopNote(8, RegularNoteType.Normal,
                3,
                5,
                3
            );

            MoveHold(9, 4, -moveX);
            await LoopNote(8, RegularNoteType.Normal,
                -3,
                -5,
                -3
            );

            MoveHold(-11, 4, moveX);
            await LoopNote(8, RegularNoteType.Normal,
                1,
                3,
                5
            );

            MoveHold(11, 4, -moveX);
            await LoopNote(8, RegularNoteType.Normal,
                -1,
                -3,
                -5
            );

            await LoopNote(16, RegularNoteType.Normal,
                0.5f,
                -0.5f,
                0.5f,
                -0.5f
            );
        }

        void MoveHold(float x, float length, float moveX)
        {

            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed);
            Vector3 startPos = new(Inv(x), StartBase);
            MoveAsync(hold, startPos, Inv(moveX)).Forget();

            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
        }

        async UniTask MoveAsync(HoldNote hold, Vector3 startPos, float moveX)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0f;
            var vec = Speed * new Vector3(moveX, -1f);
            while (hold.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                var pos = startPos + time * vec;
                hold.SetPos(pos);
                hold.SetMaskLocalPos(new Vector2(pos.x, 0));
                await Helper.Yield();
            }
        }
    }
}