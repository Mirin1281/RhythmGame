using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoteCreating
{
    /// <summary>
    /// ノーツに動きを加えるSerializeすることを前提としたインターフェース
    /// </summary>
    public interface ITransformConvertable
    {
        (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0f);
    }

    /// <summary>
    /// コマンドの動きを同期させるために使用するインターフェース
    /// </summary>
    public interface IFollowableCommand
    {
        (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0f);
    }

    /// <summary>
    /// ノーツに動きを加えるクラス
    /// </summary>
    [System.Serializable]
    public class TransformConverter
    {
        [SerializeField, SerializeReference, SubclassSelector]
        ITransformConvertable[] transformConvertables;

        public bool IsEmpty => transformConvertables == null || transformConvertables.Length == 0;

        public (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0f)
        {
            var pos = basePos;
            var rot = 0f;
            for (int i = 0; i < transformConvertables.Length; i++)
            {
                var ts = transformConvertables[i].ConvertTransform(pos, option, time);
                pos = ts.pos;
                rot += ts.rot;
            }
            return (pos, rot);
        }

#if UNITY_EDITOR
        public string GetStatus()
        {
            string moveStatus = string.Empty;
            if (transformConvertables != null && transformConvertables.Length != 0)
            {
                var firstConvertable = transformConvertables[0];
                if (firstConvertable != null)
                {
                    var className = firstConvertable.GetType().Name;
                    moveStatus = className[2..];
                }
                if (transformConvertables.Length > 1)
                {
                    moveStatus += " etc.";
                }
            }
            return moveStatus;
        }
#endif
    }


    [AddTypeMenu("左右にSinムーブ", -60), System.Serializable]
    public class P_SinMove : ITransformConvertable
    {
        [SerializeField] float amp = 7;
        [SerializeField] Lpb frequency = new Lpb(0.25f);
        [SerializeField] float startDeg = 0;

        public (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0f)
        {
            float x = basePos.x + amp * Mathf.Sin(time * (2f / frequency.Time) * Mathf.PI + startDeg * Mathf.Deg2Rad);
            float y = basePos.y;
            return (new Vector3(x, y), 0);
        }
    }

    [AddTypeMenu("2軸の揺らし", -60), System.Serializable]
    public class P_2ShakeLoop : ITransformConvertable
    {
        [SerializeField] public float deg = 3f;
        [SerializeField] public Lpb frequency = new Lpb(1f);
        [SerializeField] public Lpb phase = new Lpb(2);
        [SerializeField] public EaseType easeType = EaseType.OutQuad;

        public (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0)
        {
            float dir = GetDir(time, option != 0);
            var pos = MyUtility.GetRotatedPos(basePos, dir);
            return (pos, dir);
        }

        float GetDir(float time, bool pair)
        {
            bool isUp = pair ^ ((int)((time + phase.Time) / frequency.Time) % 2) == 0;
            float theta = (time + phase.Time) % frequency.Time;
            return (isUp ? 1 : -1) * theta.Ease(0, deg, frequency.Time / 2f, easeType);
        }
    }
}