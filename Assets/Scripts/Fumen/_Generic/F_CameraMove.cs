using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇カメラ制御"), System.Serializable]
    public class F_CameraMove : CommandBase
    {
        [SerializeField] Mirror mirror;

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと基準の時間からの差で発火します")]
        bool isChainWait = true;

        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await WaitOnTiming();
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
            Helper.CameraMover.Move(s, delta, mirror);
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }

        protected override string GetSummary()
        {
            if (settings == null || settings.Length == 0) return null;
            return $"Length: {settings.Length}";
        }
#endif
    }
}
