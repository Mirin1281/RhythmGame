using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("カーブ", -60), System.Serializable]
    public class P_Curve : ITransformConvertable
    {
        [SerializeField] bool isGroup;
        [SerializeField] float centerX = 0;
        [SerializeField] float dirSpeed = 50f;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float moveTime = new Lpb(4).Time * 6f;
            if (time <= moveTime)
            {
                float dirSpeed = option is -1 or 0 ? this.dirSpeed : option;
                Vector3 centerPos = new Vector2(centerX, 0);
                var dirEasing = new Easing(moveTime * dirSpeed, 0, moveTime, EaseType.OutQuad);
                var rot = dirEasing.Ease(time);
                item.SetRot(rot);
                item.SetPos(MyUtility.GetRotatedPos(item.GetPos(), rot, centerPos));
            }
        }
    }
}