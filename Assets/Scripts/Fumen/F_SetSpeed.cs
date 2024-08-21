using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆速度変更"), System.Serializable]
    public class F_SetSpeed : NoteGeneratorBase
    {
        [SerializeField] float speed;
        [SerializeField, Min(0)] float delay = 0f;

        protected override async UniTask GenerateAsync()
        {
            if(delay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: Helper.Token);
            }
            RhythmGameManager.Speed = speed;
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
