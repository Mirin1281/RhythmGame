using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/スカイサンプル"), System.Serializable]
    public class F_Sky1: Generator_Type1
    {
        protected override float Speed => base.Speed * 5f;

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            //CreateNormals().Forget();
            CreateSkys().Forget();
        }

        /*async UniTask CreateNormals()
        {
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Note(1, NoteType.Normal).Wait(4);
            await Wait(8);
            await Note(1, NoteType.Normal).Wait(8);
            await Note(1, NoteType.Normal).Wait(4);
        }*/

        async UniTask CreateSkys()
        {
            await SkyNote(1).Wait(4);
            await SkyNote(2).Wait(4);
            await SkyNote(1).Wait(4);
            await SkyNote(2).Wait(4);
            await SkyNote(1).Wait(4);
            await Wait(4);
            await SkyNote(2).Wait(4);
            await Wait(4);
            await SkyNote(1).Wait(4);
            await SkyNote(2).Wait(4);
            await SkyNote(1).Wait(4);
            await SkyNote(2).Wait(4);
            await SkyNote(1).Wait(4);
            await Wait(8);
            await SkyNote(2).Wait(8);
            await SkyNote(1).Wait(4);
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
    }
}
