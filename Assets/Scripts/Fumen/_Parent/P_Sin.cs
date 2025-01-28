using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("左右に揺れる"), System.Serializable]
    public class P_Sin : ParentCreatorBase
    {
        [SerializeField] float moveTime = 10f;
        [SerializeField] float amplitude = 1f;
        [SerializeField] float period = 1f;

        protected override async UniTask ExecuteAsync(RegularNote parent)
        {
            await UniTask.CompletedTask;
            SinMove(parent).Forget();
        }

        async UniTask SinMove(RegularNote parent, float delta = -1)
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
                parent.SetPos(Mir.Conv(new Vector3(amplitude * Mathf.Sin(t * period), 0)));
                await Helper.Yield();
            }
        }
    }
}