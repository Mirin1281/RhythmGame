using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/逆走"), System.Serializable]
    public class F_Lyrith_Reverse : Generator_Common
    {
        protected override async UniTask GenerateAsync()
        {
            var d = await Wait(4, 2);

            UniTask.Void(async () => 
            {
                var d1 = await Wait(8, delta: d);
                Sky(new Vector2(-4, 0), d1);
                d1 = await Wait(8, delta: d1);
                Sky(new Vector2(0, 0), d1);
                d1 = await Wait(8, delta: d1);
                Sky(new Vector2(4, 0), d1);
                d1 = await Wait(8, delta: d1);

                Sky(new Vector2(0, 4), d1);
                Sky(new Vector2(0, 0), d1);
                d1 = await Wait(4, delta: d1);
                Sky(new Vector2(-4, 2), d1);
                Sky(new Vector2(4, 2), d1);
                d1 = await Wait(4, delta: d1);
                Sky(new Vector2(0, 4), d1);
                Sky(new Vector2(0, 0), d1);
            });

            d = await Wait(4, 2, d);

            float interval = 0;
            MySky(new Vector3(-2, 4, interval), d);
            MySky(new Vector3(2, 4, interval), d);

            MySky(new Vector3(6, 0, interval + Helper.GetTimeInterval(16, 3)), d);

            MySky(new Vector3(-6, 0, interval + Helper.GetTimeInterval(4)), d);

            MySky(new Vector3(0, 0, interval + Helper.GetTimeInterval(8, 3)), d);

            MySky(new Vector3(-4, 4, interval + Helper.GetTimeInterval(2)), d);
            MySky(new Vector3(4, 4, interval + Helper.GetTimeInterval(2)), d);
        }

        void MySky(Vector3 pos, float delta)
        {
            var skyNote = Helper.GetSky();
            float moveTime = Helper.GetTimeInterval(2);
            var startPos = new Vector3(Inv(pos.x), pos.y, StartBase3D + (pos.z - Helper.GetTimeInterval(2, 3)) * Speed3D);
            Drop3DAsync(skyNote, startPos, moveTime).Forget();

            float expectTime = pos.z + moveTime - delta;
            Helper.NoteInput.AddExpect(skyNote, startPos, expectTime);


            async UniTask Drop3DAsync(NoteBase note, Vector3 startPos, float moveTime)
            {
                float baseTime = CurrentTime - delta;
                float t = 0f;
                while (note.IsActive && t < moveTime)
                {
                    t = CurrentTime - baseTime;
                    var vec = t.Ease(0, -Speed3D, moveTime / 1.95f, EaseType.OutQuad) * Vector3.back;
                    note.SetPos(startPos + t * vec);
                    await Helper.Yield();
                }

                baseTime = CurrentTime;
                var vec2 = Speed3D * Vector3.back;
                while (note.IsActive)
                {
                    t = CurrentTime - baseTime;
                    note.SetPos(startPos + t * vec2);
                    await Helper.Yield();
                }
            }
        }
    }
}