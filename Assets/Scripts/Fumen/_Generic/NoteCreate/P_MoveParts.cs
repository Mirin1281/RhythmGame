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
        [SerializeField] float deg = 3f;
        [SerializeField] Lpb frequency = new Lpb(1f);
        [SerializeField] Lpb phase = new Lpb(2);
        [SerializeField] Lpb deltaLpb = Lpb.Zero;
        [SerializeField] EaseType easeType = EaseType.OutQuad;

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
            float delta = isUp ? 0 : deltaLpb.Time;
            float theta = (time + phase.Time + delta) % frequency.Time;
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
                else
                {
                    if (note is HoldNote hold)
                    {
                        hold.SetMaskPos(hold.GetMaskPos() + new Vector2(xSpeed * basePos.y, 0));
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
                note.SetRot(note.GetRot() + d);
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
            [SerializeField] Lpb moveStartLpb = new Lpb(1);
            [SerializeField] Lpb moveLpb = new Lpb(4);
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

        [AddTypeMenu("ノーツ回転", -60), System.Serializable]
        public class P_RotateNote : ITransformConvertable
        {
            [Header("オプション : 回転係数(1で半回転)")]
            [SerializeField] bool isGroup;
            [SerializeField] Lpb timing = new Lpb(4) * 5;
            [SerializeField] Lpb rotateLpb = new Lpb(8);
            [SerializeField] EaseType easeType = EaseType.OutQuad;
            [SerializeField] float speedRate = 1f;

            bool ITransformConvertable.IsGroup => isGroup;
            float Speed => RhythmGameManager.Speed * speedRate;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (option == 0) return;

                float toRot = 180 * option;
                var easing = new Easing(0, toRot, rotateLpb.Time, easeType);
                if (time > timing.Time)
                {
                    float t2 = time - timing.Time;
                    float rot = easing.Ease(Mathf.Clamp(t2, 0, rotateLpb.Time));
                    note.SetRot(rot);

                    if (note is HoldNote hold)
                    {
                        var vec = Speed * new Vector3(Mathf.Sin(rot * Mathf.Deg2Rad), -Mathf.Cos(rot * Mathf.Deg2Rad));
                        note.SetPos(new Vector3(note.GetPos().x, 0) + t2 * vec);
                        hold.SetMaskLength(20f);
                    }
                }
            }
        }

        [AddTypeMenu("飛び上がる", -60), System.Serializable]
        public class F_JumpDrop : ITransformConvertable
        {
            [Header("オプション : 軌道の反転 (0 or 1)")]
            [SerializeField] float radius = 16.5f;
            [SerializeField] float height = 17;

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

        [AddTypeMenu("量子化", -60), System.Serializable]
        public class F_Quantization : ITransformConvertable
        {
            [Header("オプション : なし")]
            [SerializeField] Lpb startLpb = new Lpb(0);
            [SerializeField] Lpb endLpb = new Lpb(4) * 6f;
            [Space(10)]
            [SerializeField] Lpb sampling = new Lpb(32);
            [SerializeField] float speedRate = 1;

            bool ITransformConvertable.IsGroup => false;
            float Speed => speedRate * RhythmGameManager.Speed;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (time > startLpb.Time && time < endLpb.Time)
                {
                    var pos = note.GetPos();
                    var l_space = Speed * sampling.Time;
                    var snippedY = Mathf.Round(pos.y / l_space) * l_space;
                    note.SetPos(new Vector3(pos.x, snippedY));
                }


                /*var moveTime = new Lpb(4) * 6f;
                float l_interval = RhythmGameManager.DefaultSpeed / (new Lpb(4).Time * interval);
                var t = Mathf.Round(time * l_interval) / l_interval;
                var x = note.GetPos().x;
                note.SetPos(new Vector3(x, (moveTime.Time - t) * RhythmGameManager.DefaultSpeed));*/
            }
        }

        [AddTypeMenu("微細な揺れ", -60), System.Serializable]
        public class P_Distortion : ITransformConvertable
        {
            [SerializeField] bool useDefault;
            [Header("オプション1 : 揺れの係数 デフォルト1")]
            [SerializeField] bool isPosDistortion = true;
            [SerializeField] bool isRotDistortion;
            [SerializeField] float frequency = 3f;
            [SerializeField] float posAmp = 0.3f;
            [SerializeField] float rotAmp = 5;
            [SerializeField] int baseSeed = 774932;

            bool ITransformConvertable.IsGroup => false;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (useDefault)
                    option = 1;
                // new Unity.Mathematics.Random((uint)(baseSeed + note.GetHashCode()));
                var random = Unity.Mathematics.Random.CreateFromIndex((uint)(baseSeed + note.GetHashCode()));
                float randFrequency = random.NextFloat(-frequency, frequency);
                float randPhase = random.NextFloat(0, 2 * Mathf.PI);

                (Vector3 pos, float rot) moveFunc(float t)
                {
                    float addX = 0;
                    if (isPosDistortion)
                    {
                        addX = posAmp * option * Mathf.Sin(t * randFrequency + randPhase);
                    }
                    Vector3 pos = new Vector3(addX, 0);

                    float rot = 0;
                    if (isRotDistortion)
                    {
                        rot = rotAmp * option * Mathf.Sin(t * randFrequency + randPhase);
                    }
                    return (pos, rot);
                }

                var ts = moveFunc(time);
                note.SetPosAndRot(ts.pos + note.GetPos(), ts.rot);
                if (note is HoldNote hold)
                {
                    //var maskPos = hold.GetMaskPos();
                    //hold.SetMaskPos(MyUtility.GetRotatedPos(maskPos + new Vector2(ts.pos.x, 0), ts.rot));
                    hold.SetMaskRot(0);
                }
            }
        }

        [AddTypeMenu("角度変更(静的)", -60), System.Serializable]
        public class P_4Direction : ITransformConvertable
        {
            [Header("オプション: 角度")]
            [SerializeField] bool _;

            bool ITransformConvertable.IsGroup => true;

            static Vector3 ConvertPos(Vector3 pos, float dir)
            {
                pos = MyUtility.GetRotatedPos(pos, dir);
                var rad = (dir - 90) * Mathf.Deg2Rad;
                var deltaPos = new Vector3(8 * Mathf.Cos(rad), 4 * Mathf.Sin(rad) + 4f);
                return pos + deltaPos;
            }

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                float dir = option;
                note.SetRot(dir + note.GetRot());
                if (note is HoldNote hold)
                {
                    hold.SetMaskPos(ConvertPos(hold.GetMaskPos(), dir));
                    hold.SetPos(ConvertPos(hold.GetPos(), dir));
                }
                else
                {
                    note.SetPos(ConvertPos(note.GetPos(), dir));
                }
            }
        }

        [AddTypeMenu("半透明化(リバース専用)", -60), System.Serializable]
        public class P_Reverse_Transparent : ITransformConvertable
        {
            [Header("オプション: なし")]
            [SerializeField] float preAlpha = 0.3f;

            bool ITransformConvertable.IsGroup => false;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                var fadeLpb = new Lpb(2);
                var moveTime = new Lpb(4).Time * 6;
                if (time < 0)
                {
                    note.SetAlpha(preAlpha);
                }
                else if (time < fadeLpb.Time)
                {
                    var alpha = time.Ease(preAlpha, 1, fadeLpb.Time, EaseType.Default);
                    note.SetAlpha(alpha);
                }
                else
                {
                    note.SetAlpha(1);
                }
            }
        }

        [AddTypeMenu("ノーツ出現 & 消失", -60), System.Serializable]
        public class P_Appear : ITransformConvertable
        {
            [Header("オプション : 0:消失、1:出現、-1:無視\n消失させたい場合はNonJudgeと併用してください")]
            [SerializeField] bool isGroup = true;
            [SerializeField] Lpb timing = new Lpb(4) * 6;

            bool ITransformConvertable.IsGroup => isGroup;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (option == -1) return;

                bool appear = option == 1;

                if (time > timing.Time)
                {
                    note.SetRendererEnabled(appear);
                }
                else
                {
                    note.SetRendererEnabled(!appear);
                }
            }
        }

        [AddTypeMenu("Lanota", -60), System.Serializable]
        public class P_Lanota : ITransformConvertable
        {
            [SerializeField] Vector2 centerPos = new Vector2(0, 14);
            [SerializeField] Vector2 degRange = new Vector2(-45, 45);
            [SerializeField] float size = 0.5f;

            bool ITransformConvertable.IsGroup => true;

            // -8をdegRange.x、8をdegRange.yにスケーリングしています
            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                float rotFunc(float x) => (degRange.y - degRange.x) / 16f * x + (degRange.y + degRange.x) / 2f;
                Vector3 posFunc(float y, float rot) => MyUtility.GetRotatedPos(new Vector2(0, y - size), rot, centerPos);

                var basePos = note.GetPos();
                float rot = rotFunc(basePos.x);
                note.SetRot(rot + note.GetRot());
                var pos = posFunc(basePos.y, rot);
                note.SetPos(pos);

                if (note is HoldNote hold)
                {
                    var baseMaskPos = hold.GetMaskPos();
                    float maskRot = rotFunc(baseMaskPos.x);
                    hold.SetMaskRot(maskRot);
                    var maskPos = posFunc(baseMaskPos.y, rot);
                    hold.SetMaskPos(maskPos);
                }
            }
        }

        [AddTypeMenu("ノーツの設定", -60), System.Serializable]
        public class P_NoteSetting : ITransformConvertable
        {
            [SerializeField] float holdMaskLength = 5f;

            bool ITransformConvertable.IsGroup => true;

            void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
            {
                if (option == -1) return;

                if (note is HoldNote hold)
                {
                    hold.SetMaskLength(holdMaskLength);
                }
            }
        }
    }
}