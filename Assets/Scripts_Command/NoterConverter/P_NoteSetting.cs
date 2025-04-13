using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "P_Curve/P_NoteSetting")]
    [AddTypeMenu("ノーツの設定(仮)", -60), System.Serializable]
    public class P_NoteSetting : ITransformConvertable
    {
        [SerializeField] float holdMaskLength = 5f;

        bool ITransformConvertable.IsGroup => true;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (option == -1) return;

            if (item is HoldNote hold)
            {
                hold.SetMaskLength(holdMaskLength);
            }
        }
    }
}