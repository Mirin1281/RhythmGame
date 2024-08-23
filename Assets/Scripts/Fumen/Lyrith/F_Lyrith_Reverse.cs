using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/逆走"), System.Serializable]
    public class F_Lyrith_Reverse : Generator_3D
    {
        protected override async UniTask GenerateAsync()
        {
            UniTask.Void(async () => 
            {
                await Wait(8);
                Sky(new Vector2(-4, 0));
                await Wait(8);
                Sky(new Vector2(0, 0));
                await Wait(8);
                Sky(new Vector2(4, 0));
                await Wait(8);

                Sky(new Vector2(0, 4));
                Sky(new Vector2(0, 0));
                await Wait(4);
                Sky(new Vector2(-4, 2));
                Sky(new Vector2(4, 2));
                await Wait(4);
                Sky(new Vector2(0, 4));
                Sky(new Vector2(0, 0));
            });

            await Wait(2);

            float interval = 0;
            MySky(new Vector3(-2, 4, interval));
            MySky(new Vector3(2, 4, interval));
            interval += GetInterval(8);
            interval += GetInterval(16);
            MySky(new Vector3(6, 0, interval));
            interval += GetInterval(16);
            MySky(new Vector3(-6, 0, interval));
            interval += GetInterval(8);
            MySky(new Vector3(0, 0, interval));
            interval += GetInterval(8);
            MySky(new Vector3(-4, 4, interval));
            MySky(new Vector3(4, 4, interval));
        }

        float GetInterval(float lpb, int num = 1)
        {
            if(lpb == 0) return 0;
            return 240f / Helper.Metronome.Bpm / lpb * num;
        }

        void MySky(Vector3 pos)
        {
            float moveTime = GetInterval(2);
            var skyNote = Helper.SkyNotePool.GetNote();
            var startPos = new Vector3(Inverse(pos.x), pos.y, (pos.z - 0.06f) * Speed * 1.1f);
            Drop3DAsync(skyNote, startPos, moveTime).Forget();

            float expectTime = moveTime + pos.z + CurrentTime;
            var expect = new NoteExpect(skyNote, skyNote.transform.position, expectTime);
            Helper.NoteInput.AddExpect(expect);
        }

        async UniTask Drop3DAsync(NoteBase note, Vector3 startPos, float moveTime)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0f;
            var vec = Speed * Vector3.back;
            while (note.IsActive && time < moveTime / 2 + 0.2f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos - time.Ease(0f, 0.3f, moveTime / 2, EaseType.OutQuad) * vec);
                await UniTask.Yield(Helper.Token);
            }

            baseTime = CurrentTime;
            vec = Speed * 1.1f * Vector3.back;
            startPos = note.transform.localPosition;
            while (note.IsActive)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await UniTask.Yield(Helper.Token);
            }
        }
    }
}