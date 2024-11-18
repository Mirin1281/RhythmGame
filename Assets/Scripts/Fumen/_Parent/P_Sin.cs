using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("左右に揺れる"), System.Serializable]
    public class P_Sin : ParentGeneratorBase
    {
        [SerializeField] float moveTime = 10f;
        [SerializeField] float amplitude = 1f;
        [SerializeField] float period = 1f;

        protected override async UniTask MoveParentAsync(NoteBase parent)
        {
            await UniTask.CompletedTask;
            SinMove(parent).Forget();
        }

        async UniTask SinMove(NoteBase parent, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (parent.IsActive && t < moveTime)
            {
                t = CurrentTime - baseTime;
                parent.SetPos(Inv(new Vector3(amplitude * Mathf.Sin(t * period), 0)));
                await Helper.Yield();
            }
        }
    }
}