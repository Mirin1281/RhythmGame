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
        class CameraShakeStatus
        {
            public bool enable = true;
            public int beatTiming;
            public float strength = 10f;
            public float time = 0.4f;
        }

        [Header("正の値は右側(時計回り)に回転します")]
        [SerializeField] CameraShakeStatus[] statuses = new CameraShakeStatus[1];

        protected override async UniTask GenerateAsync()
        {
            var camera = Camera.main;
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
    }
}
