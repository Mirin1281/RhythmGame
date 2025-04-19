using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("横振動", -60), System.Serializable]
    public class P_SideShake : ITransformConvertable
    {
        [Header("オプション : 振動係数(デフォルト1)\nタイミングを負の値にする振動が反対になります")]
        [SerializeField] bool isGroup = true;
        [SerializeField] Lpb[] timings = new Lpb[] { new Lpb(4) * 6 };
        [SerializeField] float shakeStrength = 0.5f;
        [SerializeField] Lpb shakeLpb = new Lpb(8);
        [SerializeField] EaseType easeType = EaseType.OutQuad;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (option == 0) return;

            bool reverse = false;
            Lpb timing = Lpb.Zero;
            for (int i = 0; i < timings.Length; i++)
            {
                Lpb val = Lpb.Abs(timings[i]);
                timing += val;
                if (time < timing.Time)
                {
                    timing -= val;
                    if (i - 1 > 0 && timings[i - 1].Time < 0)
                    {
                        reverse = true;
                    }
                    break;
                }
            }
            float t = time - timing.Time;

            if (0 < t && t < shakeLpb.Time)
            {
                var easing = new Easing(option * (reverse ? -1 : 1) * shakeStrength, 0, shakeLpb.Time, easeType);
                float addX = easing.Ease(t);
                item.SetPos(item.GetPos() + new Vector3(addX, 0));
            }
        }
    }
}
