using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆3D 2D変更"), System.Serializable]
    public class F_Dimension : Generator_Type1
    {
        enum MoveType
        {
            Move2DTo3D,
            Move3DTo2D,
        }
        [SerializeField] MoveType moveType;
        [SerializeField] float time = 1f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField, Min(0)] float delay = 0f;
        [SerializeField] bool isRotateClamp = true;

        protected override async UniTask GenerateAsync()
        {
            if(delay > 0)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: Helper.Token);
            }
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            
            var rendererShower = GameObject.FindAnyObjectByType<RendererShower>(FindObjectsInactive.Include);
            if(moveType == MoveType.Move2DTo3D)
            {
                rendererShower.ShowLaneAsync(time).Forget();
                Move2DTo3D(Camera.main);
            }
            else if(moveType == MoveType.Move3DTo2D)
            {
                rendererShower.HideLaneAsync(time).Forget();
                Move3DTo2D(Camera.main);
            }
        }

        void Move2DTo3D(Camera camera)
        {
            var startPos = camera.transform.localPosition;
            var startRotate = camera.transform.localEulerAngles;
            if(isRotateClamp)
            {
                WhileYield(time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            GetEaseValue(startPos.x, 0f, t),
                            GetEaseValue(startPos.y, 7f, t),
                            GetEaseValue(startPos.z, -6.5f, t)),
                        Quaternion.Euler(
                            GetEaseValue(startRotate.x, GetNormalizedAngle(25f, startRotate.x - 180, startRotate.x + 180), t),
                            GetEaseValue(startRotate.y, GetNormalizedAngle(0, startRotate.y - 180, startRotate.y + 180), t),
                            GetEaseValue(startRotate.z, GetNormalizedAngle(0, startRotate.z - 180, startRotate.z + 180), t))
                    );
                });
            }
            else
            {
                WhileYield(time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            GetEaseValue(startPos.x, 0f, t),
                            GetEaseValue(startPos.y, 7f, t),
                            GetEaseValue(startPos.z, -6.5f, t)),
                        Quaternion.Euler(
                            GetEaseValue(startRotate.x, 25f, t),
                            GetEaseValue(startRotate.y, 0, t),
                            GetEaseValue(startRotate.z, 0, t))
                    );
                });
            }
            
        }

        void Move3DTo2D(Camera camera)
        {
            var startPos = camera.transform.localPosition;
            var startRotate = camera.transform.localEulerAngles;
            if(isRotateClamp)
            {
                WhileYield(time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            GetEaseValue(startPos.x, 0f, t),
                            GetEaseValue(startPos.y, 4f, t),
                            GetEaseValue(startPos.z, -10f, t)),
                        Quaternion.Euler(
                            GetEaseValue(startRotate.x, GetNormalizedAngle(0, startRotate.x - 180, startRotate.x + 180), t),
                            GetEaseValue(startRotate.y, GetNormalizedAngle(0, startRotate.y - 180, startRotate.y + 180), t),
                            GetEaseValue(startRotate.z, GetNormalizedAngle(0, startRotate.z - 180, startRotate.z + 180), t))
                    );
                });
            }
            else
            {
                WhileYield(time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            GetEaseValue(startPos.x, 0f, t),
                            GetEaseValue(startPos.y, 4f, t),
                            GetEaseValue(startPos.z, -10f, t)),
                        Quaternion.Euler(
                            GetEaseValue(startRotate.x, 0, t),
                            GetEaseValue(startRotate.y, 0, t),
                            GetEaseValue(startRotate.z, 0, t))
                    );
                });
            }
            
        }

        static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
        {
            return Mathf.Repeat(angle - min, max - min) + min;
        }

        float GetEaseValue(float start, float from, float t) => t.Ease(start, from, time, easeType);

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetSummary()
        {
            if(moveType == MoveType.Move2DTo3D)
            {
                return "3D";
            }
            else
            {
                return "2D";
            }
        }

        public override string CSVContent1
        {
            get
            {
                return moveType + "|" + time + "|" + easeType + "|" + delay + "|" + isRotateClamp;
            }
            set
            {
                var texts = value.Split("|");
                moveType = Enum.Parse<MoveType>(texts[0]);
                time = float.Parse(texts[1]);
                easeType = Enum.Parse<EaseType>(texts[2]);
                delay = float.Parse(texts[3]);
                isRotateClamp = bool.Parse(texts[4]);
            }
        }
    }
}
