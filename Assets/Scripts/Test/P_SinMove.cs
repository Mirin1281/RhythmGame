using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("親テスト用"), System.Serializable]
    public class P_SinMove : ParentGeneratorBase
    {
        [SerializeField] float moveTime = 10f;

        protected override async UniTask MoveParentAsync(NoteBase parent)
        {
            await UniTask.CompletedTask;
            ParentMoveAsync(parent).Forget();
        }

        async UniTask ParentMoveAsync(NoteBase parent, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (parent.IsActive && t < moveTime)
            {
                t = CurrentTime - baseTime;
                parent.SetPos(new Vector3(2f * Mathf.Sin(t * 2f), 0));
                await Helper.Yield();
            }
        }
    }
}