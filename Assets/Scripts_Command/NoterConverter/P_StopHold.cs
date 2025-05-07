using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("停止&発進ホールド"), System.Serializable]
    public class P_StopHold : ITransformConvertable
    {
        [Header("オプション : なし\n" +
            "着地の瞬間に停止し、それからResumeLpb後に動き出します")]
        [SerializeField] Lpb resumeLpb = new Lpb(4) * 6;

        bool ITransformConvertable.IsGroup => false;
        float Speed => RhythmGameManager.Speed;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (item is HoldNote hold)
            {
                float offset = 0.05f;
                float baseLength = hold.GetLength() / Speed;
                float moveTime = new Lpb(4).Time * 6 + offset;
                float length = baseLength - resumeLpb.Time;
                hold.SetLength(length * Speed);
                float resumeTime = resumeLpb.Time + moveTime;

                if (time > resumeTime)
                {
                    float t = time - resumeTime;
                    var speedEasing = new Easing(0, Speed, length, EaseType.InQuad);
                    hold.SetPos(new Vector3(hold.GetPos().x, -offset * Speed - t * speedEasing.Ease(t)));
                }
                else if (time > moveTime)
                {
                    float t = time - moveTime;
                    hold.SetPos(hold.GetPos() + new Vector3(0, t * Speed));
                }
            }
        }
    }
}