using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/P_4Direction")]
    [AddTypeMenu("角度変更(静的)", -60), System.Serializable]
    public class P_Direction : ITransformConvertable
    {
        [Header("オプション: 角度")]
        [SerializeField] bool _;

        bool ITransformConvertable.IsGroup => true;

        static Vector3 ConvertPos(Vector3 pos, float dir)
        {
            pos = MyUtility.GetRotatedPos(pos, dir);
            var rad = (dir - 90) * Mathf.Deg2Rad;
            var deltaPos = new Vector3(8 * Mathf.Cos(rad), 4 * Mathf.Sin(rad) + 4f);
            return pos + deltaPos;
        }

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float dir = option;
            item.SetRot(dir + item.GetRot());
            if (item is HoldNote hold)
            {
                hold.SetMaskPos(ConvertPos(hold.GetMaskPos(), dir));
                hold.SetPos(ConvertPos(hold.GetPos(), dir));
            }
            else
            {
                item.SetPos(ConvertPos(item.GetPos(), dir));
            }
        }
    }
}
