using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Lyrith/アーク1"), System.Serializable]
    public class F_Lyrith_Arc : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await UniTask.CompletedTask;
            /*Arc(new ArcCreateData[]
            {
                new(new(4, 4, 0), VertexType.Auto, false, false, 0, 4),
                new(new(1, 3, 16), VertexType.Auto, true),
                new(new(0, 2, 16), VertexType.Auto, true),
                new(new(1, 1, 16), VertexType.Auto, true),
                new(new(4, 0, 16), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(2, 4, 0), VertexType.Auto, false, false, 0, 8),
                new(new(-1, 3, 16), VertexType.Auto, true),
                new(new(-2, 2, 16), VertexType.Auto, false, false, 0, 8),
                new(new(-1, 1, 16), VertexType.Auto, true),
                new(new(2, 0, 16), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(0, 4, 0), VertexType.Auto, false, false, 0, 8),
                new(new(-3, 3, 16), VertexType.Auto, true),
                new(new(-4, 2, 16), VertexType.Auto, false, false, 0, 8),
                new(new(-3, 1, 16), VertexType.Auto, true),
                new(new(0, 0, 16), VertexType.Auto, true),
            });
            await Wait(4);

            Arc(new ArcCreateData[]
            {
                new(new(-2, 4, 0), VertexType.Auto, false, false, 0, 8),
                new(new(-5, 3, 16), VertexType.Auto, true),
                new(new(-6, 2, 16), VertexType.Auto, false, false, 0, 8),
                new(new(-5, 1, 16), VertexType.Auto, true),
                new(new(-2, 0, 16), VertexType.Auto, true),
            });*/
        }
    }
}
