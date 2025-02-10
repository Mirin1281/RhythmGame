using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("Lyrith/アーク2"), System.Serializable]
    public class F_Lyrith_Arc2 : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await UniTask.CompletedTask;
            /*Arc(new ArcCreateData[]
            {
                new(new(4, 4, 0), VertexType.Auto, false, false, 0, 4),
                new(new(5, 0, 12), VertexType.Auto, true),
                new(new(3, 0, 12), VertexType.Auto, true),
                new(new(-2, 0, 12), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(2, 4, 0), VertexType.Auto, false, false, 0, 4),
                new(new(3, 0, 12), VertexType.Auto, true),
                new(new(1, 0, 12), VertexType.Auto, true),
                new(new(-4, 0, 12), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(0, 4, 0), VertexType.Auto, false, false, 0, 4),
                new(new(1, 0, 12), VertexType.Auto, true),
                new(new(-1, 0, 12), VertexType.Auto, true),
                new(new(-6, 0, 12), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(-2, 4, 0), VertexType.Auto, false, false, 0, 4),
                new(new(-1, 0, 12), VertexType.Auto, true),
                new(new(-3, 0, 12), VertexType.Auto, true),
                new(new(-8, 0, 12), VertexType.Auto, true),
            });*/
        }
    }
}
