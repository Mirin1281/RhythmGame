using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Quantization")]
    [AddTypeMenu("量子化", -60), System.Serializable]
    public class P_Quantization : ITransformConvertable
    {
        [Header("オプション : なし")]
        [SerializeField] Lpb startLpb = new Lpb(0);
        [SerializeField] Lpb endLpb = new Lpb(4) * 6f;
        [Space(10)]
        [SerializeField] Lpb sampling = new Lpb(32);
        [SerializeField] float speedRate = 1;

        bool ITransformConvertable.IsGroup => false;
        float Speed => speedRate * RhythmGameManager.Speed;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (time > startLpb.Time && time < endLpb.Time)
            {
                var pos = item.GetPos();
                var l_space = Speed * sampling.Time;
                var snippedY = Mathf.Round(pos.y / l_space) * l_space;
                item.SetPos(new Vector3(pos.x, snippedY));
            }
        }
    }
}
