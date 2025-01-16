using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("Lyrith/最後の低速アーク"), System.Serializable]
    public class F_Lyrith_FinalArc : Command_General
    {
        //[SerializeField] float decay = 15f;

        protected override async UniTask ExecuteAsync()
        {
            await UniTask.CompletedTask;
            /*MyArc(new ArcCreateData[]
            {
                new(new(-6, 0, 0), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-6, 0, 4), VertexType.Linear, false, false, 0, 8),
                new(new(-6, 0, 2), VertexType.Linear, true),
            }).Forget();
            MyArc(new ArcCreateData[]
            {
                new(new(-2, 0, 0), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 2), VertexType.Linear, false, false, 0, 2),
                new(new(-2, 0, 4), VertexType.Linear, false, false, 0, 8),
                new(new(-2, 0, 2), VertexType.Linear, true),
            }).Forget();

            await Wait(4, 5); ///////////////////////////////////////////////////
            await Wait(8, 1);

            int[] waits = new int[]
            {
                12,12,16,12,16,8,16,16,8,10,8,12,8,16,12,16,24,16,24,24,24,16,24,24
            };

            for (int i = 0; i < waits.Length; i++)
            {
                var speed = Speed3D + i * 7f;
                if (i < waits.Length / 2)
                {
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(0, 0, 0), VertexType.Linear, false, false, 0, 8),
                        new(new(-6, 0, 8), VertexType.Linear, true),
                    },
                    speed);
                    await Wait(waits[i]);
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(0, 0, 0), VertexType.Linear, false, false, 0, 8),
                        new(new(-6, 0, 8), VertexType.Linear, true),
                    },
                    speed);
                    await Wait(waits[i]);
                }
                else
                {
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(6, 0, 0), VertexType.Linear, false, false, 0, 8),
                        new(new(-6, 0, 8), VertexType.Linear, true),
                    },
                    speed);
                    await Wait(waits[i]);
                    MyArc2(new ArcCreateData[]
                    {
                        new(new(6, 0, 0), VertexType.Linear, false, false, 0, 8),
                        new(new(-6, 0, 8), VertexType.Linear, true),
                    },
                    speed);
                    await Wait(waits[i]);
                }
            }*/
        }

        /*async UniTask MyArc(ArcCreateData[] datas)
        {
            var arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed3D / decay, IsInverse).Forget();
            var startPos = new Vector3(0, 0f, StartBase3D);
            bool isRanged = false;
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);
            await Helper.WaitSeconds((StartBase3D + Speed3D * Delta) / Speed3D);
            isRanged = true;


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
            }
        }

        void MyArc2(ArcCreateData[] datas, float speed)
        {
            var arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * speed, IsInverse).Forget();
            var startPos = new Vector3(0, 0f, StartBase3D);
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
                    await Helper.Yield();
                }
            }
        }

        protected override string GetName()
        {
            return "FinalArc";
        }*/
    }
}
