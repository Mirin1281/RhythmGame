using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    // TODO: Inverseに未対応
    [AddTypeMenu("◆カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_2D
    {
        public enum MoveType
        {
            Absolute,
            Relative
        }

        [Serializable]
        public class CameraMoveSetting
        {
            [SerializeField] int wait;
            [SerializeField] bool isPosMove = false;
            [SerializeField] Vector3 pos;
            [SerializeField] bool isRotateMove = true;
            [SerializeField] Vector3 rotate;
            [SerializeField, Tooltip("現在の回転から目標の回転までの差が\nより近いような回り方を選んで回転します")]
            bool isRotateClamp = true;
            [SerializeField] float time = 1f;
            [SerializeField] EaseType easeType = EaseType.OutQuad;
            [SerializeField] MoveType moveType = MoveType.Absolute;

            public int Wait => wait;
            public bool IsPosMove => isPosMove;
            public Vector3 Pos => pos;
            public bool IsRotateMove => isRotateMove;
            public Vector3 Rotate => rotate;
            public bool IsRotateClamp => isRotateClamp;
            public float Time => time;
            public EaseType EaseType => easeType;
            public MoveType MoveType => moveType;

            public CameraMoveSetting(int wait, bool isPosMove, Vector3 pos, bool isRotateMove, Vector3 rotate,
                bool isRotateClamp, float time, EaseType easeType, MoveType moveType)
            {
                this.wait = wait;
                this.isPosMove = isPosMove;
                this.pos = pos;
                this.isRotateMove = isRotateMove;
                this.rotate = rotate;
                this.isRotateClamp = isRotateClamp;
                this.time = time;
                this.easeType = easeType;
                this.moveType = moveType;
            }

            public CameraMoveSetting() {} // デフォルトコンストラクタ
        }

        [SerializeField, Min(0)] float delay = 0f;
        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTask GenerateAsync()
        {
            if(delay > 0)
            {
                await Helper.WaitSeconds(delay);
            }
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            var camera = Camera.main;
            for(int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                if(s.IsPosMove == false && s.IsRotateMove == false) return;
                if(s.MoveType == MoveType.Absolute)
                {
                    MoveAbsolute(camera, s);
                }
                else if(s.MoveType == MoveType.Relative)
                {
                    MoveRelative(camera, s);
                }
                await Wait(s.Wait);
            }
        }

        static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
        {
            return Mathf.Repeat(angle - min, max - min) + min;
        }

        void MoveAbsolute(Camera camera, CameraMoveSetting setting)
        {
            var startPos = camera.transform.position;
            var startRotate = camera.transform.eulerAngles;

            if(setting.IsPosMove && setting.IsRotateMove == false)
            {
                WhileYield(setting.Time, t => 
                {
                    camera.transform.position = new Vector3(
                        t.Ease(startPos.x, setting.Pos.x, setting.Time, setting.EaseType),
                        t.Ease(startPos.y, setting.Pos.y, setting.Time, setting.EaseType),
                        t.Ease(startPos.z, setting.Pos.z, setting.Time, setting.EaseType)
                    );
                });
            }
            else if(setting.IsPosMove == false && setting.IsRotateMove)
            {
                if(setting.IsRotateClamp)
                {
                    WhileYield(setting.Time, t => 
                    {
                        camera.transform.localRotation = Quaternion.Euler(
                            t.Ease(startRotate.x, GetNormalizedAngle(setting.Rotate.x, startRotate.x - 180, startRotate.x + 180), setting.Time, setting.EaseType),
                            t.Ease(startRotate.y, GetNormalizedAngle(setting.Rotate.y, startRotate.y - 180, startRotate.y + 180), setting.Time, setting.EaseType),
                            t.Ease(startRotate.z, GetNormalizedAngle(setting.Rotate.z, startRotate.z - 180, startRotate.z + 180), setting.Time, setting.EaseType));
                    });
                }
                else
                {
                    WhileYield(setting.Time, t => 
                    {
                        camera.transform.localRotation = Quaternion.Euler(
                            t.Ease(startRotate.x, setting.Rotate.x, setting.Time, setting.EaseType),
                            t.Ease(startRotate.y, setting.Rotate.y, setting.Time, setting.EaseType),
                            t.Ease(startRotate.z, setting.Rotate.z, setting.Time, setting.EaseType));
                    });
                }
            }
            else
            {
                if(setting.IsRotateClamp)
                {
                    WhileYield(setting.Time, t => 
                    {
                        camera.transform.SetLocalPositionAndRotation(
                            new Vector3(
                                t.Ease(startPos.x, setting.Pos.x, setting.Time, setting.EaseType),
                                t.Ease(startPos.y, setting.Pos.y, setting.Time, setting.EaseType),
                                t.Ease(startPos.z, setting.Pos.z, setting.Time, setting.EaseType)
                            ),
                            Quaternion.Euler(
                                t.Ease(startRotate.x, GetNormalizedAngle(setting.Rotate.x, startRotate.x - 180, startRotate.x + 180), setting.Time, setting.EaseType),
                                t.Ease(startRotate.y, GetNormalizedAngle(setting.Rotate.y, startRotate.y - 180, startRotate.y + 180), setting.Time, setting.EaseType),
                                t.Ease(startRotate.z, GetNormalizedAngle(setting.Rotate.z, startRotate.z - 180, startRotate.z + 180), setting.Time, setting.EaseType))
                        );
                    });
                }
                else
                {
                    WhileYield(setting.Time, t => 
                    {
                        camera.transform.SetLocalPositionAndRotation(
                            new Vector3(
                                t.Ease(startPos.x, setting.Pos.x, setting.Time, setting.EaseType),
                                t.Ease(startPos.y, setting.Pos.y, setting.Time, setting.EaseType),
                                t.Ease(startPos.z, setting.Pos.z, setting.Time, setting.EaseType)
                            ),
                            Quaternion.Euler(
                                t.Ease(startRotate.x, setting.Rotate.x, setting.Time, setting.EaseType),
                                t.Ease(startRotate.y, setting.Rotate.y, setting.Time, setting.EaseType),
                                t.Ease(startRotate.z, setting.Rotate.z, setting.Time, setting.EaseType))
                        );
                    });
                }
            }
        }

        void MoveRelative(Camera camera, CameraMoveSetting setting)
        {
            camera.transform.GetLocalPositionAndRotation(out var startPos, out var startRotate);

            if(setting.IsPosMove && setting.IsRotateMove == false)
            {
                WhileYield(setting.Time, t => 
                {
                    camera.transform.position = new Vector3(
                        t.Ease(startPos.x, setting.Pos.x + startPos.x, setting.Time, setting.EaseType),
                        t.Ease(startPos.y, setting.Pos.y + startPos.y, setting.Time, setting.EaseType),
                        t.Ease(startPos.z, setting.Pos.z + startPos.z, setting.Time, setting.EaseType)
                    );
                });
            }
            else if(setting.IsPosMove == false && setting.IsRotateMove)
            {
                WhileYield(setting.Time, t => 
                {
                    camera.transform.localRotation = startRotate * Quaternion.Euler(
                        t.Ease(0, setting.Rotate.x, setting.Time, setting.EaseType),
                        t.Ease(0, setting.Rotate.y, setting.Time, setting.EaseType),
                        t.Ease(0, setting.Rotate.z, setting.Time, setting.EaseType));
                });
            }
            else
            {
                WhileYield(setting.Time, t => 
                {
                    camera.transform.SetLocalPositionAndRotation(
                        new Vector3(
                            t.Ease(startPos.x, setting.Pos.x + startPos.x, setting.Time, setting.EaseType),
                            t.Ease(startPos.y, setting.Pos.y + startPos.y, setting.Time, setting.EaseType),
                            t.Ease(startPos.z, setting.Pos.z + startPos.z, setting.Time, setting.EaseType)
                        ),
                        startRotate * Quaternion.Euler(
                            t.Ease(0, setting.Rotate.x, setting.Time, setting.EaseType),
                            t.Ease(0, setting.Rotate.y, setting.Time, setting.EaseType),
                            t.Ease(0, setting.Rotate.z, setting.Time, setting.EaseType))
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
            get => IsInverse + "|" + delay;
            set
            {
                var texts = value.Split("|");
                SetInverse(bool.Parse(texts[0]));
                delay = float.Parse(texts[1]);
            }
        }

        public override string CSVContent2
        {
            get
            {
                string text = string.Empty;
                for(int i = 0; i < settings.Length; i++)
                {
                    var data = settings[i];
                    if(data == null) continue;
                    text += data.Wait + "|";
                    text += data.IsPosMove + "|";
                    text += data.Pos + "|";
                    text += data.IsRotateMove + "|";
                    text += data.Rotate + "|";
                    text += data.IsRotateClamp + "|";
                    text += data.Time + "|";
                    text += data.EaseType + "|";
                    text += data.MoveType;
                    if(i == settings.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var texts = value.Split("\n");
                if(texts.Length == 1 && string.IsNullOrEmpty(texts[0])) return;
                var settings = new CameraMoveSetting[texts.Length];
                for(int i = 0; i < texts.Length; i++)
                {
                    var contents = texts[i].Split('|');
                    settings[i] = new CameraMoveSetting(
                        int.Parse(contents[0]),
                        bool.Parse(contents[1]),
                        contents[2].ToVector3(),
                        bool.Parse(contents[3]),
                        contents[4].ToVector3(),
                        bool.Parse(contents[5]),
                        float.Parse(contents[6]),
                        EaseType.Parse<EaseType>(contents[7]),
                        MoveType.Parse<MoveType>(contents[8])
                    );
                }
                this.settings = settings;
            }
        }
    }
}
