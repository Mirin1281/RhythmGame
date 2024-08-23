using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆速度変更"), System.Serializable]
    public class F_SetSpeed : Generator_Type1
    {
        [SerializeField] float speedRate = 1f;
        [SerializeField, Min(0)] float delaySeconds;

        protected override async UniTask GenerateAsync()
        {
            if(delaySeconds > 0)
            {
                await WaitSeconds(delaySeconds + Delta);
            }
            RhythmGameManager.SetSpeed(speedRate);
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get
            {
                return speedRate + "|" + delaySeconds;
            }
            set
            {
                var texts = value.Split("|");
                speedRate = float.Parse(texts[0]);
                delaySeconds = float.Parse(texts[1]);
            }
        }
    }
}
