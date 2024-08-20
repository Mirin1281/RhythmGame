using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Lyrith_CameraShake")]
    [AddTypeMenu("カメラを揺らす"), System.Serializable]
    public class F_CameraShake : Generator_Type1
    {
        [System.Serializable]
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
            if(statuses == null) return;
            Camera camera = Camera.main;
            int count = 0;
            int startBeatCount = Helper.Metronome.BeatCount;
            Beat(startBeatCount, default);
            Helper.Metronome.OnBeat += Beat;
            await UniTask.CompletedTask;


            void Beat(int beatCount, float _)
            {
                var status = statuses[count];
                if(beatCount >= startBeatCount + status.beatTiming)
                {
                    if(status.enable)
                    {
                        CameraShake(camera, status.strength, status.time);
                    }
                    
                    count++;
                    if(count == statuses.Length)
                    {
                        Helper.Metronome.OnBeat -= Beat;
                        return;
                    }
                }
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
            if(statuses == null) return null;
            return statuses.Length.ToString();
        }
    }
}
