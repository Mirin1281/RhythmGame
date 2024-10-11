using Cysharp.Threading.Tasks;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/アーク2"), System.Serializable]
    public class F_Lyrith_Arc2 : Generator_Common
    {
        protected override async UniTask GenerateAsync()
        {
            Arc(new ArcCreateData[]
            {
                new(new(4, 4, 0), ArcVertexMode.Auto, false, false, 0, 4),
                new(new(5, 0, 12), ArcVertexMode.Auto, true),
                new(new(3, 0, 12), ArcVertexMode.Auto, true),
                new(new(-2, 0, 12), ArcVertexMode.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(2, 4, 0), ArcVertexMode.Auto, false, false, 0, 4),
                new(new(3, 0, 12), ArcVertexMode.Auto, true),
                new(new(1, 0, 12), ArcVertexMode.Auto, true),
                new(new(-4, 0, 12), ArcVertexMode.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(0, 4, 0), ArcVertexMode.Auto, false, false, 0, 4),
                new(new(1, 0, 12), ArcVertexMode.Auto, true),
                new(new(-1, 0, 12), ArcVertexMode.Auto, true),
                new(new(-6, 0, 12), ArcVertexMode.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(-2, 4, 0), ArcVertexMode.Auto, false, false, 0, 4),
                new(new(-1, 0, 12), ArcVertexMode.Auto, true),
                new(new(-3, 0, 12), ArcVertexMode.Auto, true),
                new(new(-8, 0, 12), ArcVertexMode.Auto, true),
            });
        }
    }
}
