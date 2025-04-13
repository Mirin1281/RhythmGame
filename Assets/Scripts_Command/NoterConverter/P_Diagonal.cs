using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "P_Curve/P_Diagonal")]
    [AddTypeMenu("角度をつけて落下", -60), System.Serializable]
    public class P_Diagonal : ITransformConvertable
    {
        [SerializeField] bool setRotate;
        [SerializeField] bool useDefault = true;
        [SerializeField] float defaultDir = 10;

        bool ITransformConvertable.IsGroup => false;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float dir = useDefault ? defaultDir : option;
            float xSpeed = Mathf.Cos((dir + 90) * Mathf.Deg2Rad);
            var basePos = item.GetPos();
            item.SetPos(new Vector3(basePos.x + xSpeed * basePos.y, basePos.y));
            if (setRotate)
            {
                item.SetRot(dir);
                if (item is HoldNote hold)
                {
                    hold.SetMaskRot(0);
                }
            }
            else
            {
                if (item is HoldNote hold)
                {
                    hold.SetMaskPos(hold.GetMaskPos() + new Vector2(xSpeed * basePos.y, 0));
                }
            }
        }
    }
}
