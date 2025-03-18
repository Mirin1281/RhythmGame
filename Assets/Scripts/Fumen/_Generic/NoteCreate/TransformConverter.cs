using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoteCreating
{
    /// <summary>
    /// ノーツに動きを加えるインターフェース
    /// </summary>
    interface ITransformConvertable
    {
        (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option, float time);
        bool IsGroup { get; }
        void Init() { }
    }

    /// <summary>
    /// ノーツに動きを加えるクラス 
    /// </summary>
    [System.Serializable]
    public class TransformConverter
    {
        [Header(nameof(NoteData) + "のオプション1はConverterの要素0、オプション2は要素1以降に適用されます")]

        [SerializeField, SerializeReference, SubclassSelector]
        ITransformConvertable[] transformConvertables;
        bool initialized = false;

        public (Vector3 pos, float rot) Convert(Vector3 basePos, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0)
        {
            if (initialized == false)
            {
                Init();
            }
            if (transformConvertables == null) return (basePos, 0);
            var pos = basePos;
            float rot = 0;
            for (int i = 0; i < transformConvertables.Length; i++)
            {
                var convertable = transformConvertables[i];
                if (convertable == null) continue;
                var ts = convertable.ConvertTransform(pos, i == 0 ? option1 : option2, convertable.IsGroup ? groupTime : unGroupTime);
                pos = ts.pos;
                rot += ts.rot;
            }
            return (pos, rot);
        }

        void Init()
        {
            if (transformConvertables == null) return;
            for (int i = 0; i < transformConvertables.Length; i++)
            {
                var convertable = transformConvertables[i];
                if (convertable == null) continue;
                convertable.Init();
            }
            initialized = true;
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

    /// <summary>
    /// コマンドの動きを同期させるために使用するインターフェース
    /// </summary>
    public interface IFollowableCommand
    {
        (Vector3 pos, float rot) ConvertTransform(
            Vector3 basePos, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0);
    }
}