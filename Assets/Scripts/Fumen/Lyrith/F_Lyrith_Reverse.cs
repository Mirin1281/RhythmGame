using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/逆走"), System.Serializable]
    public class F_Lyrith_Reverse : Generator_3D
    {
        protected override async UniTask GenerateAsync()
        {
            await Wait(4, 2);

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

            await Wait(4, 2);

            float interval = 0;
            MySky(new Vector3(-2, 4, interval));
            MySky(new Vector3(2, 4, interval));
            interval += Helper.GetTimeInterval(8);
            MySky(new Vector3(6, 0, interval));
            interval += Helper.GetTimeInterval(16);
            MySky(new Vector3(-6, 0, interval));
            interval += Helper.GetTimeInterval(8);
            MySky(new Vector3(0, 0, interval));
            interval += Helper.GetTimeInterval(8);
            MySky(new Vector3(-4, 4, interval));
            MySky(new Vector3(4, 4, interval));
        }

        void MySky(Vector3 pos)
        {
            var skyNote = Helper.GetSky();
            float timeInterval = pos.z;
            var startPos = new Vector3(Inverse(pos.x), pos.y, timeInterval * Speed * 1.1f);
            float moveTime = Helper.GetTimeInterval(2);
            Drop3DAsync(skyNote, startPos, moveTime).Forget();

            float expectTime = CurrentTime + moveTime + timeInterval;
            var expect = new NoteExpect(skyNote, startPos, expectTime);
            Helper.NoteInput.AddExpect(expect);


            async UniTask Drop3DAsync(NoteBase note, Vector3 startPos, float moveTime)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = Speed * Vector3.back;
                while (note.IsActive && time < moveTime / 2f + 0.2f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos - time.Ease(0f, 0.3f, moveTime / 2, EaseType.OutQuad) * vec);
                    await Helper.Yield();
                }

                baseTime = CurrentTime;
                vec = Speed * 1.1f * Vector3.back;
                startPos = note.GetPos();
                while (note.IsActive)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }
            }
        }
    }
}