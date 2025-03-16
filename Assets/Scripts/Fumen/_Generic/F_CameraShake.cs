using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇カメラを揺らす"), System.Serializable]
    public class F_CameraShake : CommandBase
    {
        public enum ShakeType
        {
            Rotate,
            Vertical,
            Horizontal,
            Random,
            Vibration_Horizontal,
        }

        [SerializeField] Mirror mirror;
        [SerializeField, Min(0)] int loopCount = 16;
        [SerializeField] Lpb loopWait = new Lpb(2);

        [Space(20)]
        [SerializeField] ShakeType shakeType = ShakeType.Rotate;
        [SerializeField] float intensity = 2f;
        [SerializeField] Lpb time = new Lpb(4);
        [SerializeField, Tooltip("Randomの場合シード値、Vibration_Horizontalの場合振動数を表す")] int option = 100;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb, Delta - RhythmGameManager.Offset);
            for (int i = 0; i < loopCount; i++)
            {
                ShakeCamera();
                await Wait(loopWait);
            }
        }

        void ShakeCamera()
        {
            if (shakeType == ShakeType.Rotate)
            {
                Helper.CameraMover.Move(null, new Vector3(0, 0, intensity), CameraMoveType.Relative, new Lpb(0), EaseType.OutQuad, delta: Delta, mir: mirror);
                Helper.CameraMover.Move(null, new Vector3(0, 0, -intensity), CameraMoveType.Relative, time, EaseType.OutQuad, delta: Delta, mir: mirror);
            }
            else if (shakeType == ShakeType.Vertical)
            {
                Helper.CameraMover.Move(new Vector3(0, intensity), null, CameraMoveType.Relative, new Lpb(0), EaseType.OutQuad, delta: Delta, mir: mirror);
                Helper.CameraMover.Move(new Vector3(0, -intensity), null, CameraMoveType.Relative, time, EaseType.OutQuad, delta: Delta, mir: mirror);
            }
            else if (shakeType == ShakeType.Horizontal)
            {
                Helper.CameraMover.Move(new Vector3(-intensity, 0), null, CameraMoveType.Relative, new Lpb(0), EaseType.OutQuad, delta: Delta, mir: mirror);
                Helper.CameraMover.Move(new Vector3(intensity, 0), null, CameraMoveType.Relative, time, EaseType.OutQuad, delta: Delta, mir: mirror);
            }
            else if (shakeType == ShakeType.Random)
            {
                var rand = new System.Random(option);
                var ampEasing = new Easing(intensity, 0, time.Time, EaseType.OutCubic);
                WhileYield(time.Time, t =>
                {
                    Vector3 pos = ampEasing.Ease(t) * new Vector3(rand.GetFloat(-intensity, intensity), rand.GetFloat(-intensity, intensity));
                    Helper.CameraMover.MoveRelative(pos, default);
                }, Delta, PlayerLoopTiming.PreLateUpdate);
            }
            else if (shakeType == ShakeType.Vibration_Horizontal)
            {
                var ampEasing = new Easing(intensity, 0, time.Time, EaseType.OutCubic);
                WhileYield(time.Time, t =>
                {
                    float x = Mathf.Cos(t * option) * ampEasing.Ease(t);
                    Helper.CameraMover.MoveRelative(new Vector3(x, 0), default);
                }, Delta, PlayerLoopTiming.PreLateUpdate);
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }

        protected override string GetSummary()
        {
            return $"{loopCount} - {loopWait.GetLpbValue()} :  {shakeType}";
        }
#endif
    }
}
