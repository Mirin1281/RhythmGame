using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/P_RotateNote")]
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

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (option == 0) return;

            float toRot = 180 * option;
            var easing = new Easing(0, toRot, rotateLpb.Time, easeType);
            if (time > timing.Time)
            {
                float t2 = time - timing.Time;
                float rot = easing.Ease(Mathf.Clamp(t2, 0, rotateLpb.Time));
                item.SetRot(rot);

                if (item is HoldNote hold)
                {
                    var vec = Speed * new Vector3(Mathf.Sin(rot * Mathf.Deg2Rad), -Mathf.Cos(rot * Mathf.Deg2Rad));
                    item.SetPos(new Vector3(item.GetPos().x, 0) + t2 * vec);
                    hold.SetMaskLength(20f);
                }
            }
        }
    }
}
