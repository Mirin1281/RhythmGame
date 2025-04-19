using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("カーブ", -60), System.Serializable]
    public class P_Curve : ITransformConvertable
    {
        [SerializeField] bool isGroup;
        [SerializeField] float centerX = 0;
        [SerializeField] float dirSpeed = 50f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float moveTime = new Lpb(4).Time * 6f;
            if (time <= moveTime)
            {
                float dirSpeed = option is -1 or 0 ? this.dirSpeed : option;
                Vector3 centerPos = new Vector2(centerX, 0);
                var dirEasing = new Easing(moveTime * dirSpeed, 0, moveTime, easeType);
                var rot = dirEasing.Ease(time);
                item.SetRot(rot);
                item.SetPos(MyUtility.GetRotatedPos(item.GetPos(), rot, centerPos));
            }
        }
    }
}