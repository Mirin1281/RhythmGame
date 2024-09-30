using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆カメラを揺らす"), System.Serializable]
    public class F_CameraShake : Generator_2D
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

            public CameraShakeSetting(int wait = 0, bool disabled = false, float strength = 10f, float time = 0.4f)
            {
                this.wait = wait;
                this.disabled = disabled;
                this.strength = strength;
                this.time = time;
            }
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
            return settings?.Length.ToString();
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get => IsInverse.ToString();
            set { SetInverse(bool.Parse(value)); }
        }

        public override string CSVContent2
        {
            get
            {
                string text = string.Empty;
                for(int i = 0; i < settings.Length; i++)
                {
                    var data = settings[i];
                    text += data.Wait + "|";
                    text += data.Disabled + "|";
                    text += data.Strength + "|";
                    text += data.Time;
                    if(i == settings.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var texts = value.Split("\n");
                if(texts.Length == 1 && string.IsNullOrEmpty(texts[0])) return;
                var settings = new CameraShakeSetting[texts.Length];
                for(int i = 0; i < texts.Length; i++)
                {
                    var contents = texts[i].Split('|');
                    settings[i] = new CameraShakeSetting(
                        int.Parse(contents[0]),
                        bool.Parse(contents[1]),
                        float.Parse(contents[2]),
                        float.Parse(contents[3])
                    );
                }
                this.settings = settings;
            }
        }
    }
}
