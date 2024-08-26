using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_2D
    {
        enum MoveType
        {
            Absolute,
            Relative
        }

        [SerializeField] bool isPosMove = false;
        [SerializeField] Vector3 pos;
        [SerializeField] bool isRotateMove = true;
        [SerializeField] Vector3 rotate;
        [SerializeField] bool isRotateClamp = true;
        [SerializeField] float time = 1f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] MoveType moveType = MoveType.Absolute;
        [SerializeField, Min(0)] float delay = 0f;

        protected override async UniTask GenerateAsync()
        {
            if(isPosMove == false && isRotateMove == false) return;
            if(delay > 0)
            {
                await WaitSeconds(delay);
            }
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            var camera = Camera.main;
            if(moveType == MoveType.Absolute)
            {
                MoveAbsolute(camera, time);
            }
            else if(moveType == MoveType.Relative)
            {
                MoveRelative(camera, time);
            }
        }

        static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
        {
            return Mathf.Repeat(angle - min, max - min) + min;
        }

        void MoveAbsolute(Camera camera, float time)
        {
            var startPos = camera.transform.position;
            var startRotate = camera.transform.eulerAngles;

            if(isPosMove && isRotateMove == false)
            {
                WhileYield(time, t => 
                {
                    camera.transform.position = new Vector3(
                        t.Ease(startPos.x, pos.x, time, easeType),
                        t.Ease(startPos.y, pos.y, time, easeType),
                        t.Ease(startPos.z, pos.z, time, easeType)
                    );
                });
            }
            else if(isPosMove == false && isRotateMove)
            {
                if(isRotateClamp)
                {
                    WhileYield(time, t => 
                    {
                        camera.transform.localRotation = Quaternion.Euler(
                            t.Ease(startRotate.x, GetNormalizedAngle(rotate.x, startRotate.x - 180, startRotate.x + 180), time, easeType),
                            t.Ease(startRotate.y, GetNormalizedAngle(rotate.y, startRotate.y - 180, startRotate.y + 180), time, easeType),
                            t.Ease(startRotate.z, GetNormalizedAngle(rotate.z, startRotate.z - 180, startRotate.z + 180), time, easeType));
                    });
                }
                else
                {
                    WhileYield(time, t => 
                    {
                        camera.transform.localRotation = Quaternion.Euler(
                            t.Ease(startRotate.x, rotate.x, time, easeType),
                            t.Ease(startRotate.y, rotate.y, time, easeType),
                            t.Ease(startRotate.z, rotate.z, time, easeType));
                    });
                }
            }
            else
            {
                if(isRotateClamp)
                {
                    WhileYield(time, t => 
                    {
                        camera.transform.SetLocalPositionAndRotation(
                            new Vector3(
                                t.Ease(startPos.x, pos.x, time, easeType),
                                t.Ease(startPos.y, pos.y, time, easeType),
                                t.Ease(startPos.z, pos.z, time, easeType)
                            ),
                            Quaternion.Euler(
                                t.Ease(startRotate.x, GetNormalizedAngle(rotate.x, startRotate.x - 180, startRotate.x + 180), time, easeType),
                                t.Ease(startRotate.y, GetNormalizedAngle(rotate.y, startRotate.y - 180, startRotate.y + 180), time, easeType),
                                t.Ease(startRotate.z, GetNormalizedAngle(rotate.z, startRotate.z - 180, startRotate.z + 180), time, easeType))
                        );
                    });
                }
                else
                {
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
                                t.Ease(startRotate.z, rotate.z, time, easeType))
                        );
                    });
                }
            }
        }

        void MoveRelative(Camera camera, float time)
        {
            camera.transform.GetLocalPositionAndRotation(out var startPos, out var startRotate);

            if(isPosMove && isRotateMove == false)
            {
                WhileYield(time, t => 
                {
                    camera.transform.position = new Vector3(
                        t.Ease(startPos.x, pos.x + startPos.x, time, easeType),
                        t.Ease(startPos.y, pos.y + startPos.y, time, easeType),
                        t.Ease(startPos.z, pos.z + startPos.z, time, easeType)
                    );
                });
            }
            else if(isPosMove == false && isRotateMove)
            {
                WhileYield(time, t => 
                {
                    camera.transform.localRotation = startRotate * Quaternion.Euler(
                        t.Ease(0, rotate.x, time, easeType),
                        t.Ease(0, rotate.y, time, easeType),
                        t.Ease(0, rotate.z, time, easeType));
                });
            }
            else
            {
                WhileYield(time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            t.Ease(startPos.x, pos.x + startPos.x, time, easeType),
                            t.Ease(startPos.y, pos.y + startPos.y, time, easeType),
                            t.Ease(startPos.z, pos.z + startPos.z, time, easeType)
                        ),
                        startRotate * Quaternion.Euler(
                            t.Ease(0, rotate.x, time, easeType),
                            t.Ease(0, rotate.y, time, easeType),
                            t.Ease(0, rotate.z, time, easeType))
                    );
                });
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get
            {
                return isPosMove + "|" + pos + "|" + isRotateMove + "|" + rotate + "|" +
                    isRotateClamp + "|" + time + "|" + easeType + "|" + moveType + "|" + delay;
            }
            set
            {
                var texts = value.Split("|");
                isPosMove = bool.Parse(texts[0]);
                pos = texts[1].ToVector3();
                isRotateMove = bool.Parse(texts[2]);
                rotate = texts[3].ToVector3();
                isRotateClamp = bool.Parse(texts[4]);
                time = float.Parse(texts[5]);
                easeType = Enum.Parse<EaseType>(texts[6]);
                moveType = Enum.Parse<MoveType>(texts[7]);
                delay = float.Parse(texts[8]);
            }
        }
    }
}
