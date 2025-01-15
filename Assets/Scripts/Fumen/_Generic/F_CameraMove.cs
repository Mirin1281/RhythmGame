using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◇カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_Common
    {
        [SerializeField, Min(0)] int loopCount = 1;
        [SerializeField, Min(0)] float loopWait = 4;

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと基準の時間からの差で発火します")]
        bool isChainWait = true;
        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTask GenerateAsync()
        {
            float delta = await WaitOnTiming();
            for (int i = 0; i < loopCount; i++)
            {
                if (isChainWait)
                {
                    LoopMove(settings, delta).Forget();
                }
                else
                {
                    for (int k = 0; k < settings.Length; k++)
                    {
                        Move(settings[k], delta).Forget();
                    }
                }
                delta = await Wait(loopWait, delta);
            }
        }

        async UniTaskVoid LoopMove(CameraMoveSetting[] settings, float delta)
        {
            for (int i = 0; i < settings.Length; i++)
            {
                delta = await Wait(settings[i].Wait, delta);
                Move(settings[i], delta, true).Forget();
            }
        }

        async UniTaskVoid Move(CameraMoveSetting s, float delta, bool isDisableWait = false)
        {
            if (!isDisableWait)
            {
                delta = await Wait(s.Wait, delta: delta);
            }
            Helper.CameraMover.Move(s, delta, IsInverse);
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetSummary()
        {
            if (settings == null || settings.Length == 0) return null;
            return $"{loopCount} - {loopWait}  Length: {settings.Length}";
        }
    }
}
