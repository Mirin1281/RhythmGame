using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "P_Curve/P_Lanota")]
    [AddTypeMenu("Lanota", -60), System.Serializable]
    public class P_Lanota : ITransformConvertable
    {
        [SerializeField] Vector2 centerPos = new Vector2(0, 14);
        [SerializeField] Vector2 degRange = new Vector2(-45, 45);
        [SerializeField] float size = 0.5f;

        bool ITransformConvertable.IsGroup => true;

        // -8をdegRange.x、8をdegRange.yにスケーリングしています
        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float rotFunc(float x) => (degRange.y - degRange.x) / 16f * x + (degRange.y + degRange.x) / 2f;
            Vector3 posFunc(float y, float rot) => MyUtility.GetRotatedPos(new Vector2(0, y - size), rot, centerPos);

            var basePos = item.GetPos();
            float rot = rotFunc(basePos.x);
            item.SetRot(rot + item.GetRot());
            var pos = posFunc(basePos.y, rot);
            item.SetPos(pos);

            if (item is HoldNote hold)
            {
                var baseMaskPos = hold.GetMaskPos();
                float maskRot = rotFunc(baseMaskPos.x);
                hold.SetMaskRot(maskRot);
                var maskPos = posFunc(baseMaskPos.y, rot);
                hold.SetMaskPos(maskPos);
            }
        }
    }
}

