using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("ノーツ出現 & 消失", -60), System.Serializable]
    public class P_Appear : ITransformConvertable
    {
        [Header("オプション : 0:消失、1:出現、-1:無視\n消失させたい場合はNonJudgeと併用してください")]
        [SerializeField] bool isGroup = true;
        [SerializeField] Lpb timing = new Lpb(4) * 6;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (option == -1) return;

            bool appear = option == 1;

            if (time > timing.Time)
            {
                item.SetRendererEnabled(appear);
            }
            else
            {
                item.SetRendererEnabled(!appear);
            }
        }
    }
}