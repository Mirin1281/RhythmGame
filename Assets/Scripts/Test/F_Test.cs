using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : Generator_2D
    {
        protected override async UniTask GenerateAsync()
        {
            /*for(int i = 0; i < 18; i++)
            {
                NoteBase_2D n = Helper.GetNote2D(NoteType.Normal);
                float dir = (i * 20 + 90) * Mathf.Deg2Rad;
                n.SetPos(new Vector3(3, 0) + StartBase * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                n.SetRotate(i * 20);
            }*/
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 0f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 20f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 40f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 180f);
            await Wait(4);
        }

        void RotateNote(Vector2 pos, NoteType type, float deg)
        {
            NoteBase_2D note = Helper.GetNote2D(type);
            note.SetRotate(deg);
            note.SetWidth(2);
            Vector2 startPos = MyUtility.GetRotatedPos(new Vector2(Inverse(pos.x), StartBase + pos.y), deg, pos);
            RotateDropAsync(note, startPos, deg).Forget();

            float distance = StartBase - Speed * Delta;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, new Vector2(Inverse(pos.x), 0), expectTime);
            Helper.NoteInput.AddExpect(expect);


            async UniTask RotateDropAsync(NoteBase_2D note, Vector2 startPos, float deg)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = new Vector3(0, -Speed, 0);
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + MyUtility.GetRotatedPos(time * vec, deg));
                    await Helper.Yield();
                }
            }
        }
    }
}