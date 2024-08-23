using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Lyrith_CameraShake")]
    [AddTypeMenu("◆カメラを揺らす"), System.Serializable]
    public class F_CameraShake : Generator_Type1
    {
        [Serializable]
        struct CameraShakeStatus
        {
            public bool enable;
            public int beatTiming;
            public float strength;
            public float time;

            public CameraShakeStatus(bool enable = true, int beatTiming = 0, float strength = 10f, float time = 0.4f)
            {
                this.enable = enable;
                this.beatTiming = beatTiming;
                this.strength = strength;
                this.time = time;
            }
        }

        [Header("正の値は右側(時計回り)に回転します")]
        [SerializeField] CameraShakeStatus[] statuses = new CameraShakeStatus[1];

        protected override async UniTask GenerateAsync()
        {
            var camera = Camera.main;
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            int count = 0;
            int beatCount = 0;
            Beat(default, default);
            Helper.Metronome.OnBeat += Beat;
            await UniTask.CompletedTask;


            void Beat(int _, float __)
            {
                var status = statuses[count];
                if(beatCount == status.beatTiming)
                {
                    if(status.enable)
                    {
                        CameraShake(camera, status.strength, status.time);
                    }
                    count++;
                    if(count == statuses.Length)
                    {
                        Helper.Metronome.OnBeat -= Beat;
                    }
                }
                beatCount++;
            }
        }

        void CameraShake(Camera camera, float strength, float time)
        {
            camera.transform.localRotation = Quaternion.Euler(0f, 0f, strength);
            WhileYield(time, t => 
            {
                camera.transform.localRotation = Quaternion.Euler(0f, 0f, t.Ease(strength, 0, time, EaseType.OutBack));
            });
        }

        protected override string GetSummary()
        {
            return statuses.Length.ToString();
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get
            {
                string text = string.Empty;
                text += IsInverse + "\n";
                for(int i = 0; i < statuses.Length; i++)
                {
                    var data = statuses[i];
                    text += data.enable + "|";
                    text += data.beatTiming + "|";
                    text += data.strength + "|";
                    text += data.time;
                    if(i == statuses.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var texts = value.Split("\n");
                SetInverse(bool.Parse(texts[0]));
                var statuses = new CameraShakeStatus[texts.Length - 1];
                for(int i = 0; i < texts.Length - 1; i++)
                {
                    var contents = texts[i + 1].Split('|');
                    statuses[i] = new CameraShakeStatus(
                        bool.Parse(contents[0]),
                        int.Parse(contents[1]),
                        float.Parse(contents[2]),
                        float.Parse(contents[3]));
                }
                this.statuses = statuses;
            }
        }
    }
}
