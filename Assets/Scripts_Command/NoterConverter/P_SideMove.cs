using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("途中で横移動", -60), System.Serializable]
    public class P_SideMove : ITransformConvertable
    {
        [Header("オプション : 移動前のx座標")]
        [SerializeField] bool isGroup = false;
        [SerializeField] Lpb moveStartLpb = new Lpb(1);
        [SerializeField] Lpb moveLpb = new Lpb(4);
        [SerializeField] EaseType easeType = EaseType.OutQuad;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            var basePos = item.GetPos();
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

            item.SetPos(new Vector3(x, basePos.y));

            if (item is HoldNote hold)
            {
                hold.SetMaskPos(new Vector2(x, hold.GetMaskPos().y));
            }
        }
    }
}
