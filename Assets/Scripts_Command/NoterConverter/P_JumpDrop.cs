using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_JumpDrop")]
    [AddTypeMenu("飛び上がる", -60), System.Serializable]
    public class P_JumpDrop : ITransformConvertable
    {
        [Header("オプション : 軌道の反転 (0 or 1)")]
        [SerializeField] float radius = 16.5f;
        [SerializeField] float height = 17;

        bool ITransformConvertable.IsGroup => false;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            float moveTime = new Lpb(4).Time * 6f;
            var basePos = item.GetPos();
            if (time < moveTime)
            {
                bool reverse = option == 1;
                float centerX = reverse ? 2 * basePos.x : 0;
                float dir = reverse
                    ? -basePos.y / radius + Mathf.PI
                    : basePos.y / radius;
                item.SetPos(new Vector3(basePos.x * Mathf.Cos(dir) + centerX, height * Mathf.Sin(dir)));
            }
        }
    }
}
