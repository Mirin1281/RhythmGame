using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/スカイ2"), System.Serializable]
    public class F_Lyrith_Sky2: Generator_Type1
    {
        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            await SkyLoop(16,
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4),
                (0, 4)
            );
        }

        Generator_Type1 SkyNote(float x, float y = 4f)
        {
            var skyNote = Helper.NormalNotePool.GetNote(1);
            var startPos = new Vector3(GetInverse(x), y, StartBase);
            DropAsync(skyNote, startPos).Forget();

            float distance = startPos.z - From - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            var expect = new NoteExpect(skyNote, skyNote.transform.position, expectTime);
            Helper.NoteInput.AddExpect(expect);
            return this;


            async UniTask DropAsync(NoteBase note, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = Speed * Vector3.back;
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        async UniTask<float> SkyLoop(float lpb, params (float x, float y)?[] poses)
        {
            for(int i = 0; i < poses.Length; i++)
            {
                if(poses[i] is (float, float) pos)
                {
                    SkyNote(pos.x, pos.y);
                }
                await Wait(lpb);
            }
            return Delta;
        }
    }
}
