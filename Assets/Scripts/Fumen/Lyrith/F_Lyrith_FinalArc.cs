using Cysharp.Threading.Tasks;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/最後の低速アーク"), System.Serializable]
    public class F_Lyrith_FinalArc : Generator_3D
    {
        [SerializeField] float decay = 15f;
        
        protected override async UniTask GenerateAsync()
        {
            MyArc(new ArcCreateData[]
            {
                new(new(-6, 0, 0), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-6, 0, 4), ArcVertexMode.Linear, false, 0, 8),
                new(new(-6, 0, 2), ArcVertexMode.Linear),
            },
            ArcColorType.Blue).Forget();
            MyArc(new ArcCreateData[]
            {
                new(new(-2, 0, 0), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 2), ArcVertexMode.Linear, false, 0, 2),
                new(new(-2, 0, 4), ArcVertexMode.Linear, false, 0, 8),
                new(new(-2, 0, 2), ArcVertexMode.Linear),
            },
            ArcColorType.Red).Forget();

            await Wait(4, 5); ///////////////////////////////////////////////////
            await Wait(8, 1);

            int[] waits = new int[]
            {
                12,12,16,12,16,8,16,16,8,10,8,12,8,16,12,16,24,16,24,24,24,16,24,24
            };

            for(int i = 0; i < waits.Length; i++)
            {
                var speed = Speed + i * 7f;
                if(i < waits.Length / 2)
                {
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(0, 0, 0), ArcVertexMode.Linear, false, 0, 8),
                        new(new(-6, 0, 8), ArcVertexMode.Linear),
                    },
                    ArcColorType.Red, speed);
                    await Wait(waits[i]);
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(0, 0, 0), ArcVertexMode.Linear, false, 0, 8),
                        new(new(-6, 0, 8), ArcVertexMode.Linear),
                    },
                    ArcColorType.Blue, speed);
                    await Wait(waits[i]);
                }
                else
                {
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(6, 0, 0), ArcVertexMode.Linear, false, 0, 8),
                        new(new(-6, 0, 8), ArcVertexMode.Linear),
                    },
                    ArcColorType.Red, speed);
                    await Wait(waits[i]);
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(6, 0, 0), ArcVertexMode.Linear, false, 0, 8),
                        new(new(-6, 0, 8), ArcVertexMode.Linear),
                    },
                    ArcColorType.Blue, speed);
                    await Wait(waits[i]);
                }
            }
        }

        async UniTask MyArc(ArcCreateData[] datas, ArcColorType colorType)
        {
            var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, Speed / decay, IsInverse).Forget();
            arc.SetColor(colorType, IsInverse);
            var startPos = new Vector3(0, 0f, StartBase);
            bool isRanged = false;
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);
            await WaitSeconds((StartBase + Speed * Delta) / Speed);
            isRanged = true;            


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
            }
        }

        void MyArc2(ArcCreateData[] datas, ArcColorType colorType, float speed)
        {
            var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, speed, IsInverse).Forget();
            arc.SetColor(colorType, IsInverse);
            var startPos = new Vector3(0, 0f, StartBase);
            bool isRanged = false;
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);           


            async UniTask DropAsync(ArcNote arc, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                var vec = speed * Vector3.back;
                while (arc.IsActive && isRanged == false)
                {
                    float time = CurrentTime - baseTime;
                    arc.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        protected override string GetName()
        {
            return "FinalArc";
        }
    }
}
