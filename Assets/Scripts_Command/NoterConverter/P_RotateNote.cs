using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/P_RotateNote")]
    [AddTypeMenu("ノーツ回転", -60), System.Serializable]
    public class P_RotateNote : ITransformConvertable
    {
        enum TimingType { Once, Detailed }

        [Header("オプション : 回転係数(1で半回転)\nタイミングを負の値にすると回転が反対になります")]
        [SerializeField] bool isGroup;
        [SerializeField] TimingType timingType = TimingType.Once;
        [SerializeField] Lpb timing = new Lpb(4) * 5;
        [SerializeField] Lpb[] timings = new Lpb[] { new Lpb(4) * 5 };
        [SerializeField] Lpb rotateLpb = new Lpb(8);
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] float speedRate = 1f;

        bool ITransformConvertable.IsGroup => isGroup;
        float Speed => RhythmGameManager.Speed * speedRate;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (option == 0) return;

            float t;
            bool reverse = false;
            if (timingType == TimingType.Once)
            {
                t = time - timing.Time;
                if (timing.Time < 0)
                {
                    reverse = true;
                }
            }
            else
            {
                Lpb tim = Lpb.Zero;
                for (int i = 0; i < timings.Length; i++)
                {
                    Lpb val = Lpb.Abs(timings[i]);
                    tim += val;
                    if (time < tim.Time)
                    {
                        tim -= val;
                        if (i - 1 > 0 && timings[i - 1].Time < 0)
                        {
                            reverse = true;
                        }
                        break;
                    }
                }
                t = time - tim.Time;
            }

            if (0 < t && t < rotateLpb.Time)
            {
                var easing = new Easing(0, 180 * option * (reverse ? -1 : 1), rotateLpb.Time, easeType);
                float rot = easing.Ease(t);
                item.SetRot(rot);

                if (item is HoldNote hold)
                {
                    var vec = Speed * new Vector3(Mathf.Sin(rot * Mathf.Deg2Rad), -Mathf.Cos(rot * Mathf.Deg2Rad));
                    item.SetPos(new Vector3(item.GetPos().x, 0) + t * vec);
                    hold.SetMaskLength(20f);
                }
            }
        }
    }
}
