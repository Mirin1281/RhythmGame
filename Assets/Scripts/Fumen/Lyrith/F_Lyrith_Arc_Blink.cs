using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/アークとスカイ 点滅"), System.Serializable]
    public class F_Lyrith_Arc_Blink : Generator_Common
    {
        [SerializeField] float decay = 3f;
        List<ArcNote> arcs;
        List<SkyNote> skys;
        
        protected override async UniTask GenerateAsync()
        {
            arcs = new(2);
            skys = new(4);

            MyArc(new ArcCreateData[]
            {
                new(new(6, 0, 0), ArcVertexMode.Linear, false, false, 0, 3),
                new(new(0, 0, 3), ArcVertexMode.Linear, true),
            }, true).Forget();
            MyArc(new ArcCreateData[]
            {
                new(new(-6, 0, 0), ArcVertexMode.Linear, false, false, 0, 3),
                new(new(0, 0, 3), ArcVertexMode.Linear, true),
            }).Forget();

            await Wait(2);

            MySky(-2, 0, true).Forget();
            MySky(2, 0).Forget();

            await Wait(4);

            MySky(-4, 0).Forget();
            MySky(4, 0).Forget();
        }

        async UniTask BlinkLoop()
        {
            var rand = new System.Random(223);
            float baseTime = CurrentTime;
            float time = 0f;
            float interval = 1 / 120f;
            while(time < 1f)
            {
                time = CurrentTime - baseTime;
                await Helper.WaitSeconds(interval * rand.Next(1, 5));

                arcs.ForEach(arc => arc.SetRendererEnabled(false));
                skys.ForEach(sky => sky.SetRendererEnabled(false));
                await Helper.WaitSeconds(interval * rand.Next(1, 5));

                arcs.ForEach(arc => arc.SetRendererEnabled(true));
                skys.ForEach(sky => sky.SetRendererEnabled(true));
            }
        }

        async UniTask MyArc(ArcCreateData[] datas, bool first = false)
        {
            var arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed3D / decay, IsInverse).Forget();

            var startPos = new Vector3(0, 0f, StartBase3D);
            bool isRanged = false;
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);

            await Helper.WaitSeconds((StartBase3D + Speed3D * Delta) / Speed3D);
            if(first)
            {
                BlinkLoop().Forget();
            }
            isRanged = true;
            arcs.Add(arc);
            


            async UniTask DropAsync(ArcNote arc, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                var vec = Speed3D * Vector3.back;
                while (arc.IsActive && isRanged == false)
                {
                    float time = CurrentTime - baseTime;
                    arc.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }

                baseTime = CurrentTime;
                vec = Speed3D / decay * Vector3.back;
                startPos = arc.transform.localPosition;
                while (arc.IsActive)
                {
                    float time = CurrentTime - baseTime;
                    arc.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }
                arcs.Remove(arc);
                arc.SetRendererEnabled(true);
            }
        }

        async UniTask MySky(float x, float y = 4f, bool first = false)
        {
            var skyNote = Helper.GetSky();
            var startPos = new Vector3(Inv(x), y, StartBase3D);
            DropAsync3D(skyNote, startPos).Forget();
            skys.Add(skyNote);

            float expectTime = startPos.z/Speed3D - Delta;
            Helper.NoteInput.AddExpect(skyNote, startPos, expectTime);

            await Helper.WaitSeconds(expectTime);
            if(first)
            {
                skys.ForEach(sky => sky.SetRendererEnabled(true));
                skys.Clear();
            }
        }

        protected override string GetName()
        {
            return "アーク点滅";
        }
    }
}
