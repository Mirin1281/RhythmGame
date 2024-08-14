using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/1_1 円ノーツサンプル"), System.Serializable]
    public class F_Lyrith1_1 : Generator_Type1
    {
        float interval2 => 120f / Helper.Metronome.Bpm;
        protected override async UniTask GenerateAsync()
        {
            await Wait(1);

            await Wait(4);
            await LoopCreateCircle(4,
                (3, 3),
                (3, -3),
                (-3, -3),
                (-3, 3),
                null,
                (0, 0),
                null
            );
        }

        void CreateCircle(Vector2 pos)
        {
            var note = Helper.NormalNotePool.GetNote(2);
            CircleMoveAsync(note, pos).Forget();
            Helper.NoteInput.AddExpect(new NoteExpect(note, pos, CurrentTime + interval2));


            async UniTask CircleMoveAsync(NoteBase note, Vector3 startPos)
            {
                note.SetPos(startPos);
                float baseTime = CurrentTime - Delta;
                float t = 0f;
                while (note.IsActive && t < 3f)
                {
                    t = CurrentTime - baseTime;
                    note.transform.localScale = Vector3.one * t.Ease(1.5f, -0.1f, interval2, EaseType.InQuad);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        async UniTask LoopCreateCircle(int lpb, params (int x, int y)?[] poses)
        {
            for(int i = 0; i < poses.Length; i++)
            {
                if(poses[i] is (int, int) pos)
                {
                    CreateCircle(new Vector2(pos.x, pos.y));
                }
                await Wait(lpb);
            }
        }
    }
}
