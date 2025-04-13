using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/P_Revolute")]
    [AddTypeMenu("座標移動", -60), System.Serializable]
    public class P_Transform : ITransformConvertable
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

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
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
            item.SetRot(item.GetRot() + d);
            item.SetPos(MyUtility.GetRotatedPos(item.GetPos(), d) + p);
            if (item is HoldNote hold)
            {
                hold.SetMaskPos(MyUtility.GetRotatedPos(hold.GetMaskPos(), d) + p);
            }
        }
    }
}
