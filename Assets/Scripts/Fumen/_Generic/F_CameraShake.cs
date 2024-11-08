using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◇カメラを揺らす"), System.Serializable]
    public class F_CameraShake : Generator_Common
    {
        [Serializable]
        struct CameraShakeSetting
        {
            [SerializeField] int wait;
            [SerializeField] bool disabled;
            [SerializeField] float strength;
            [SerializeField] float time;

            public readonly int Wait => wait;
            public readonly bool Disabled => disabled;
            public readonly float Strength => strength;
            public readonly float Time => time;
        }

        [Header("正の値は右側(時計回り)に回転します")]
        [SerializeField] CameraShakeSetting[] settings;

        protected override async UniTask GenerateAsync()
        {
            if(settings == null) return;
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                if(s.Disabled == false)
                {
                    Helper.CameraMover.Shake(s.Strength, s.Time, isInverse: IsInverse);
                }
                await Wait(s.Wait);
            }
        }

        protected override string GetSummary()
        {
            return settings?.Length + GetInverseSummary();
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
