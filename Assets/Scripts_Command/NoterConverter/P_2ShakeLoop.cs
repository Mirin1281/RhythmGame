using UnityEngine;

namespace NoteCreating
{
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

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float dir = GetDir(time, option != 0);
            item.SetRot(dir);
            var pos = MyUtility.GetRotatedPos(item.GetPos(), dir);
            item.SetPos(pos);

            if (item is HoldNote hold)
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
}