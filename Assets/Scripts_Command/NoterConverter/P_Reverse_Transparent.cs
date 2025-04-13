using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/P_Reverse_Transparent")]
    [AddTypeMenu("半透明化(リバース専用)", -60), System.Serializable]
    public class P_Reverse_Transparent : ITransformConvertable
    {
        [Header("オプション: なし")]
        [SerializeField] float preAlpha = 0.3f;

        bool ITransformConvertable.IsGroup => false;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            var fadeLpb = new Lpb(2);
            var moveTime = new Lpb(4).Time * 6;
            if (time < 0)
            {
                item.SetAlpha(preAlpha);
            }
            else if (time < fadeLpb.Time)
            {
                var alpha = time.Ease(preAlpha, 1, fadeLpb.Time, EaseType.Default);
                item.SetAlpha(alpha);
            }
            else
            {
                item.SetAlpha(1);
            }
        }
    }
}