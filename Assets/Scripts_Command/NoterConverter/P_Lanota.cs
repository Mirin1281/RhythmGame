using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("Lanota", -60), System.Serializable]
    public class P_Lanota : ITransformConvertable
    {
        [SerializeField] Vector2 basePos;
        [SerializeField] Vector2 degRange = new Vector2(-35, 35);
        [SerializeField] float size = 14f;

        bool ITransformConvertable.IsGroup => true;

        // -8をdegRange.x、8をdegRange.yにスケーリングしています
        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float rotFunc(float x) => (degRange.y - degRange.x) / 16f * x + (degRange.y + degRange.x) / 2f;
            Vector3 posFunc(float y, float rot) => MyUtility.GetRotatedPos(new Vector2(0, y), rot, new Vector2(0, size)) + this.basePos;

            var basePos = item.GetPos();
            float rot = rotFunc(basePos.x);
            item.SetRot(rot + item.GetRot());
            var pos = posFunc(basePos.y, rot);
            item.SetPos(pos);

            if (item is HoldNote hold)
            {
                var baseMaskPos = hold.GetMaskPos();
                hold.SetMaskRot(rot + hold.GetMaskRot());
                var maskPos = posFunc(baseMaskPos.y, rot);
                hold.SetMaskPos(maskPos);
            }
        }
    }
}

