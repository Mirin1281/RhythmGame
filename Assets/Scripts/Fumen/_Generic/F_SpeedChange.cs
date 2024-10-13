using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆速度変更"), System.Serializable]
    public class F_SpeedChange : NoteGeneratorBase
    {
        [Header("通常を1として、全体のノーツスピードを変更します")]
        [SerializeField] float speed = 1f;
        [SerializeField] float easeTime;
        [SerializeField] EaseType easeType = EaseType.Linear;

        protected override async UniTask GenerateAsync()
        {
            var easing = new Easing(RhythmGameManager.SpeedBase, speed, easeTime, easeType);
            float baseTime = CurrentTime - Delta;
            float t = 0f;
            while(t < easeTime)
            {
                t = CurrentTime - baseTime;
                SetSpeed(easing.Ease(t));
                await UniTask.Yield(Helper.Token);
            }
            SetSpeed(easing.Ease(easeTime));


            static void SetSpeed(float value)
            {
                RhythmGameManager.SpeedBase = value;
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetName()
        {
            return "速度変更";
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(speed, easeTime, easeType);
            set
            {
                var texts = value.Split('|');

                speed = float.Parse(texts[0]);
                easeTime = float.Parse(texts[1]);
                easeType = Enum.Parse<EaseType>(texts[2]);
            }
        }
    }
}