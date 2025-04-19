using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("速度変更", -60), System.Serializable]
    public class P_Speed : ITransformConvertable
    {
        // 高さを変更するだけなので、回転後とかには非対応

        [Header("オプション : 速さ(非イージング時)")]
        [SerializeField] bool isGroup;

        // イージングの設定
        [SerializeField] bool useEasing = false;
        [SerializeField] float yRate = 1;
        [SerializeField] EaseType easeType = EaseType.InCubic;
        [SerializeField] Lpb moveLpb = new Lpb(4) * 6;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            var pos = item.GetPos();
            if (useEasing)
            {
                var posEasing = new Easing(RhythmGameManager.Speed * yRate, 0, moveLpb.Time, easeType);
                item.SetPos(new Vector3(pos.x, posEasing.Ease(time)));
            }
            else
            {
                item.SetPos(new Vector3(pos.x, pos.y * option));
            }
        }
    }
}
