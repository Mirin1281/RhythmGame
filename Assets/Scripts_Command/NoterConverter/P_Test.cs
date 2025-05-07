using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("テスト"), System.Serializable]
    public class P_Test : ITransformConvertable
    {
        [SerializeField] bool isGroup;

        bool ITransformConvertable.IsGroup => isGroup;
        float Speed => RhythmGameManager.Speed;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (item is HoldNote hold)
            {
                float offset = 0.05f;
                Lpb length = new Lpb(4);
                hold.SetLength(length * Speed);
                float moveTime = new Lpb(4).Time * 6 + offset;
                float resumeTime = new Lpb(4).Time * 6 + new Lpb(4).Time * 6;

                if (time > resumeTime)
                {
                    float t = time - resumeTime;
                    var speedEasing = new Easing(0, Speed, length.Time, EaseType.InQuad);
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