using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/◆カメラ移動"), System.Serializable]
    public class F_CameraAngle : Generator_Type1
    {
        enum MoveType
        {
            Move2DTo3D,
            Move3DTo2D,
        }
        [SerializeField] MoveType moveType;
        [SerializeField] float time = 1f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        
        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            if(moveType == MoveType.Move2DTo3D)
            {
                Move2DTo3D();
            }
            else if(moveType == MoveType.Move3DTo2D)
            {
                Move3DTo2D();
            }
        }

        void Move2DTo3D()
        {
            var camera = Camera.main;
            var startPos = camera.transform.localPosition;
            var startRotate = camera.transform.localEulerAngles;
            WhileYield(time, t => 
            {
                camera.transform.SetLocalPositionAndRotation(
                    new Vector3(
                        GetEaseValue(startPos.x, 0f, t),
                        GetEaseValue(startPos.y, 7f, t),
                        GetEaseValue(startPos.z, -6.5f, t)),
                    Quaternion.Euler(
                        GetEaseValue(startRotate.x, 25f, t),
                        GetEaseValue(startRotate.y, 0f, t),
                        GetEaseValue(startRotate.z, 0f, t)));
            });
        }

        void Move3DTo2D()
        {
            var camera = Camera.main;
            var startPos = camera.transform.localPosition;
            var startRotate = camera.transform.localEulerAngles;
            WhileYield(time, t => 
            {
                camera.transform.SetLocalPositionAndRotation(
                    new Vector3(
                        GetEaseValue(startPos.x, 0f, t),
                        GetEaseValue(startPos.y, 4f, t),
                        GetEaseValue(startPos.z, -10f, t)),
                    Quaternion.Euler(
                        GetEaseValue(startRotate.x, 0f, t),
                        GetEaseValue(startRotate.y, 0f, t),
                        GetEaseValue(startRotate.z, 0f, t)));
            });
        }

        float GetEaseValue(float start, float from, float t) => t.Ease(start, from, time, easeType);
    }
}
