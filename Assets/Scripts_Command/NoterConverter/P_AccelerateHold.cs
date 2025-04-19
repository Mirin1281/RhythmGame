using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", "P_Curve/F_AccelerateHold")]
    [AddTypeMenu("加速するホールド", -60), System.Serializable]
    public class P_AccelerateHold : ITransformConvertable
    {
        [Header("オプション : ホールド終端時の速度(デフォルト1)")]
        [SerializeField] bool useDefault;
        [SerializeField] float defaultAccelerate = 2f;
        [SerializeField] EaseType easeType = EaseType.InQuad;
        [SerializeField] float speedRate = 1f;

        bool ITransformConvertable.IsGroup => false;
        float Speed => speedRate * RhythmGameManager.Speed;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (item is HoldNote hold)
            {
                if (useDefault)
                    option = defaultAccelerate;
                var length = hold.GetLength();
                hold.SetLength(length * option);

                var speedEasing = new Easing(0, (option - 1) * Speed, length / Speed, easeType);

                float moveTime = new Lpb(4).Time * 6f;
                if (time < moveTime)
                {

                }
                else
                {
                    float t2 = time - moveTime;
                    item.SetPos(item.GetPos() + new Vector3(0, -t2 * speedEasing.Ease(t2)));
                }
            }
        }
    }
}
