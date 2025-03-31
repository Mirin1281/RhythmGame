using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("左右にSinムーブ", -60), System.Serializable]
    public class P_SinMove : ITransformConvertable
    {
        [SerializeField] bool isGroup = true;
        [SerializeField] float amp = 7;
        [SerializeField] Lpb frequency = new Lpb(0.25f);
        [SerializeField] float startDeg = 0;

        bool ITransformConvertable.IsGroup => isGroup;

        public void ConvertNote(RegularNote note, float option, float time)
        {
            var addX = amp * Mathf.Sin(time * (2f / frequency.Time) * Mathf.PI + startDeg * Mathf.Deg2Rad);
            var pos = note.GetPos() + new Vector3(addX, 0);
            note.SetPos(pos);

            if (note is HoldNote hold)
            {
                var maskPos = hold.GetMaskPos() + new Vector2(addX, 0);
                hold.SetMaskPos(maskPos);
            }
        }
    }

    [AddTypeMenu("2軸の揺らし", -60), System.Serializable]
    public class P_2ShakeLoop : ITransformConvertable
    {
        [Header("オプション: 2軸のどちらに属するか (0 or 1)")]
        [SerializeField] public float deg = 3f;
        [SerializeField] public Lpb frequency = new Lpb(1f);
        [SerializeField] public Lpb phase = new Lpb(2);
        [SerializeField] public EaseType easeType = EaseType.OutQuad;

        bool ITransformConvertable.IsGroup => true;

        void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
        {
            float dir = GetDir(time, option != 0);
            note.SetRot(dir);
            var pos = MyUtility.GetRotatedPos(note.GetPos(), dir);
            note.SetPos(pos);

            if (note is HoldNote hold)
            {
                var maskPos = MyUtility.GetRotatedPos(hold.GetMaskPos(), dir);
                hold.SetMaskRot(dir);
                hold.SetMaskPos(maskPos);
            }
        }

        float GetDir(float time, bool pair)
        {
            bool isUp = pair ^ ((int)((time + phase.Time) / frequency.Time) % 2) == 0;
            float theta = (time + phase.Time) % frequency.Time;
            return (isUp ? 1 : -1) * theta.Ease(0, deg, frequency.Time / 2f, easeType);
        }
    }

    [AddTypeMenu("カーブ", -60), System.Serializable]
    public class P_Curve : ITransformConvertable
    {
        [SerializeField] bool isGroup;
        [SerializeField] float centerX = 0;
        [SerializeField] float dirSpeed = 50f;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
        {
            float moveTime = new Lpb(4).Time * 6f;
            if (time <= moveTime)
            {
                float dirSpeed = option is -1 or 0 ? this.dirSpeed : option;
                Vector3 centerPos = new Vector2(centerX, 0);
                var dirEasing = new Easing(moveTime * dirSpeed, 0, moveTime, EaseType.OutQuad);
                var rot = dirEasing.Ease(time);
                note.SetRot(rot);
                note.SetPos(MyUtility.GetRotatedPos(note.GetPos(), rot, centerPos));
            }
        }

        [AddTypeMenu("角度をつけて落下", -60), System.Serializable]
        public class P_Diagonal : ITransformConvertable
        {
            [SerializeField] bool setRotate;
            [SerializeField] bool useDefault = true;
            [SerializeField] float defaultDir = 10;

            bool ITransformConvertable.IsGroup => false;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                float dir = useDefault ? defaultDir : option;
                float xSpeed = Mathf.Cos((dir + 90) * Mathf.Deg2Rad);
                var basePos = note.GetPos();
                note.SetPos(new Vector3(basePos.x + xSpeed * basePos.y, basePos.y));
                if (setRotate)
                {
                    note.SetRot(dir);
                    if (note is HoldNote hold)
                    {
                        hold.SetMaskRot(0);
                    }
                }
            }
        }

        [AddTypeMenu("座標移動", -60), System.Serializable]
        public class P_Revolute : ITransformConvertable
        {
            [SerializeField] bool isGroup = true;
            [SerializeField] Lpb moveStartLpb = new Lpb(4) * 6f;
            [SerializeField] Lpb easeLpb = new Lpb(1);
            [SerializeField] EaseType easeType = EaseType.Default;
            [SerializeField] float startDeg;
            [SerializeField] float toDeg;
            [SerializeField] Vector2 startPos;
            [SerializeField] Vector2 toPos;

            bool ITransformConvertable.IsGroup => isGroup;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                var easeTime = easeLpb.Time;
                var dEasing = new Easing(startDeg, toDeg, easeTime, easeType);
                var pEasing = new EasingVector2(startPos, toPos, easeTime, easeType);

                time -= moveStartLpb.Time;

                float d;
                Vector2 p;
                if (time < easeTime)
                {
                    float t = Mathf.Clamp(time, 0, easeTime);
                    d = dEasing.Ease(t);
                    p = pEasing.Ease(t);
                }
                else
                {
                    d = toDeg;
                    p = toPos;
                }
                note.SetRot(d);
                note.SetPos(MyUtility.GetRotatedPos(note.GetPos(), d) + p);
                if (note is HoldNote hold)
                {
                    hold.SetMaskPos(MyUtility.GetRotatedPos(hold.GetMaskPos(), d) + p);
                }
            }
        }

        [AddTypeMenu("途中で横移動", -60), System.Serializable]
        public class P_SideMove : ITransformConvertable
        {
            [Header("オプション : 移動前のx座標")]
            [SerializeField] bool isGroup = false;
            [SerializeField] Lpb moveLpb = new Lpb(4);
            [SerializeField] Lpb moveStartLpb = new Lpb(1);
            [SerializeField] EaseType easeType = EaseType.OutQuad;

            bool ITransformConvertable.IsGroup => isGroup;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                var basePos = note.GetPos();
                var easing = new Easing(option, basePos.x, moveLpb.Time, easeType);
                float x;
                if (time < moveStartLpb.Time)
                {
                    x = option;
                }
                else if (time < moveStartLpb.Time + moveLpb.Time)
                {
                    float t2 = time - moveStartLpb.Time;
                    x = easing.Ease(t2);
                }
                else
                {
                    x = basePos.x;
                }
                note.SetPos(new Vector3(x, basePos.y));
            }
        }

        [AddTypeMenu("ノーツ回転(Group = false)", -60), System.Serializable]
        public class P_RotateNote : ITransformConvertable
        {
            [Header("オプション : 回転係数(1で半回転)")]
            [SerializeField] bool isGroup;
            [SerializeField] Lpb timing = new Lpb(4) * 5;
            [SerializeField] Lpb rotateLpb = new Lpb(8);
            [SerializeField] EaseType easeType = EaseType.OutQuad;

            bool ITransformConvertable.IsGroup => isGroup;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                float toRot = 180 * option;
                var easing = new Easing(0, toRot, rotateLpb.Time, easeType);
                if (time > timing.Time)
                {
                    float t2 = time - timing.Time;
                    note.SetRot(easing.Ease(Mathf.Clamp(t2, 0, rotateLpb.Time)));
                }
            }
        }

        [AddTypeMenu("飛び上がる", -60), System.Serializable]
        public class F_JumpDrop : ITransformConvertable
        {
            [Header("オプション : 軌道の反転 (0 or 1)")]
            [SerializeField] float radius = 14;
            [SerializeField] float height = 15;

            bool ITransformConvertable.IsGroup => false;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                float moveTime = new Lpb(4).Time * 6f;
                var basePos = note.GetPos();
                if (time < moveTime)
                {
                    bool reverse = option == 1;
                    float centerX = reverse ? 2 * basePos.x : 0;
                    float dir = reverse
                        ? -basePos.y / radius + Mathf.PI
                        : basePos.y / radius;
                    note.SetPos(new Vector3(basePos.x * Mathf.Cos(dir) + centerX, height * Mathf.Sin(dir)));
                }
            }
        }

        [AddTypeMenu("加速するホールド", -60), System.Serializable]
        public class F_AccelerateHold : ITransformConvertable
        {
            [Header("オプション : ホールド終端時の速度(デフォルト1)")]
            [SerializeField] EaseType easeType = EaseType.InQuad;
            [SerializeField] float speedRate = 1f;

            bool ITransformConvertable.IsGroup => false;
            float Speed => speedRate * RhythmGameManager.Speed;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (note.Type == RegularNoteType.Hold)
                {
                    var hold = note as HoldNote;
                    var length = hold.GetLength();
                    hold.SetLength(length * option);

                    var speedEasing = new Easing(0, (option - 1) * Speed, length / Speed, easeType);

                    float moveTime = new Lpb(4).Time * 6f;
                    if (time < moveTime)
                    {

                    }
                    else
                    {
                        float t2 = time - moveTime;
                        note.SetPos(note.GetPos() + new Vector3(0, -t2 * speedEasing.Ease(t2)));
                    }
                }
            }
        }
    }
}