using UnityEngine;

namespace NoteCreating
{
    /// <summary>
    /// ノーツに動きを加えるインターフェース
    /// </summary>
    public interface ITransformConvertable
    {
        void ConvertItem(ItemBase item, float option, float time);
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

        public void Convert(ItemBase item, Mirror mirror, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0)
        {
            if (initialized == false)
            {
                Init();
            }
            if (transformConvertables == null) return;
            for (int i = 0; i < transformConvertables.Length; i++)
            {
                var convertable = transformConvertables[i];
                if (convertable == null) continue;
                convertable.ConvertItem(item, i == 0 ? option1 : option2, convertable.IsGroup ? groupTime : unGroupTime);
            }

            item.SetPos(mirror.Conv(item.GetPos()));
            item.SetRot(mirror.Conv(item.GetRot()));
            if (item is HoldNote hold)
            {
                hold.SetMaskPos(mirror.Conv(hold.GetMaskPos()));
            }
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
        void ConvertItem(ItemBase item, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0);
    }
}