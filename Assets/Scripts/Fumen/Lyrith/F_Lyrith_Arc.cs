using Cysharp.Threading.Tasks;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/アーク1"), System.Serializable]
    public class F_Lyrith_Arc : Generator_Type1
    {
        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            CreateArc(new ArcCreateData[]
            {
                new(new(4, 4, 0), ArcVertexMode.Auto, false, 0, 4),
                new(new(0, 2, 8), ArcVertexMode.Auto, true, 0, 8),
                new(new(4, 0, 8), ArcVertexMode.Auto),
            }, ArcColorType.Red);
            await Wait(4);

            CreateArc(new ArcCreateData[]
            {
                new(new(2, 4, 0), ArcVertexMode.Auto, false, 0, 8),
                new(new(-2, 2, 8), ArcVertexMode.Auto, false, 0, 8),
                new(new(2, 0, 8), ArcVertexMode.Auto),
            }, ArcColorType.Blue);
            await Wait(4);

            CreateArc(new ArcCreateData[]
            {
                new(new(0, 4, 0), ArcVertexMode.Auto, false, 0, 8),
                new(new(-4, 2, 8), ArcVertexMode.Auto, false, 0, 8),
                new(new(0, 0, 8), ArcVertexMode.Auto),
            }, ArcColorType.Red);
            await Wait(4);

            CreateArc(new ArcCreateData[]
            {
                new(new(-2, 4, 0), ArcVertexMode.Auto, false, 0, 8),
                new(new(-6, 2, 8), ArcVertexMode.Auto, false, 0, 8),
                new(new(-2, 0, 8), ArcVertexMode.Auto),
            }, ArcColorType.Blue);
        }
    }
}
