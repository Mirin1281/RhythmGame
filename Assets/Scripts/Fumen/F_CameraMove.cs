using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_Type1
    {
        [SerializeField] Vector3 pos;
        [SerializeField] Vector3 rotate;
        [SerializeField] float time = 1f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField, Min(0)] float delay = 0f;

        protected override async UniTask GenerateAsync()
        {
            if(delay > 0)
            {
                await WaitSeconds(delay);
            }
            var camera = Camera.main;
            var startPos = camera.transform.localPosition;
            var startRotate = camera.transform.localEulerAngles;
            WhileYield(time, t => 
            {
                camera.transform.SetLocalPositionAndRotation(
                    new Vector3(
                        t.Ease(startPos.x, pos.x, time, easeType),
                        t.Ease(startPos.y, pos.y, time, easeType),
                        t.Ease(startPos.z, pos.z, time, easeType)
                    ),
                    Quaternion.Euler(
                        t.Ease(startRotate.x, rotate.x, time, easeType),
                        t.Ease(startRotate.y, rotate.y, time, easeType),
                        t.Ease(startRotate.z, rotate.z, time, easeType)
                    )
                );
            });
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }
    }
}
