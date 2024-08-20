using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/アークとスカイ 点滅"), System.Serializable]
    public class F_Lyrith_Arc_Blink : Generator_Type1
    {
        [SerializeField] float decay = 3f;
        List<ArcNote> arcs;
        List<SkyNote> skys;
        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            arcs = new(2);
            skys = new(4);

            CreateArc(new ArcCreateData[]
            {
                new(new(6, 0, 0), ArcVertexMode.Linear, false, 0, 3),
                new(new(0, 0, 3), ArcVertexMode.Linear),
            }, ArcColorType.Red, true).Forget();
            CreateArc(new ArcCreateData[]
            {
                new(new(-6, 0, 0), ArcVertexMode.Linear, false, 0, 3),
                new(new(0, 0, 3), ArcVertexMode.Linear),
            }, ArcColorType.Blue).Forget();

            await Wait(2);

            SkyNote(-2, 0, true).Forget();
            SkyNote(2, 0).Forget();

            await Wait(4);

            SkyNote(-4, 0).Forget();
            SkyNote(4, 0).Forget();
        }

        async UniTask BlinkLoop()
        {
            var rand = new System.Random(223);
            float baseTime = CurrentTime;
            float time = 0f;
            while(time < 1f)
            {
                time = CurrentTime - baseTime;
                await UniTask.DelayFrame(rand.Next(1, 5), cancellationToken: Helper.Token);

                arcs.ForEach(arc => arc.SetRendererEnabled(false));
                skys.ForEach(sky => sky.SetRendererEnabled(false));
                await UniTask.DelayFrame(rand.Next(1, 5), cancellationToken: Helper.Token);

                arcs.ForEach(arc => arc.SetRendererEnabled(true));
                skys.ForEach(sky => sky.SetRendererEnabled(true));
            }
        }

        async UniTask CreateArc(ArcCreateData[] datas, ArcColorType colorType, bool first = false)
        {
            var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, Speed / decay, IsInverse).Forget();
            arc.SetColor(colorType, IsInverse);

            var startPos = new Vector3(0, 0f, StartBase);
            bool isRanged = false;
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);

            await UniTask.Delay(System.TimeSpan.FromSeconds((StartBase + Speed * Delta) / Speed), cancellationToken: Helper.Token);
            if(first)
            {
                BlinkLoop().Forget();
            }
            isRanged = true;
            arcs.Add(arc);
            


            async UniTask DropAsync(ArcNote arc, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                var vec = Speed * Vector3.back;
                while (arc.IsActive && isRanged == false)
                {
                    float time = CurrentTime - baseTime;
                    arc.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }

                baseTime = CurrentTime;
                vec = Speed / decay * Vector3.back;
                startPos = arc.transform.localPosition;
                while (arc.IsActive)
                {
                    float time = CurrentTime - baseTime;
                    arc.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }
                arcs.Remove(arc);
                arc.SetRendererEnabled(true);
            }
        }

        async UniTask SkyNote(float x, float y = 4f, bool first = false)
        {
            var skyNote = Helper.SkyNotePool.GetNote();
            var startPos = new Vector3(GetInverse(x), y, StartBase);
            DropAsync(skyNote, startPos).Forget();
            skys.Add(skyNote);

            float distance = startPos.z - From - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            var expect = new NoteExpect(skyNote, skyNote.transform.position, expectTime);
            Helper.NoteInput.AddExpect(expect);

            await UniTask.Delay(System.TimeSpan.FromSeconds(distance / Speed), cancellationToken: Helper.Token);
            if(first)
            {
                skys.ForEach(sky => sky.SetRendererEnabled(true));
                skys.Clear();
            }

            async UniTask DropAsync(SkyNote sky, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = Speed * Vector3.back;
                while (sky.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    sky.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        protected override string GetName()
        {
            return "アーク点滅";
        }
    }
}
