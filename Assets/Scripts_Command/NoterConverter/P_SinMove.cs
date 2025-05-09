using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("左右にSinムーブ", -60), System.Serializable]
    public class P_SinMove : ITransformConvertable
    {
        [Header("オプション : 揺れの係数(デフォルト1)")]
        [SerializeField] bool isGroup = true;
        [SerializeField] bool useDefault = true;
        [SerializeField] float amp = 7;
        [SerializeField] Lpb frequency = new Lpb(0.25f);
        [SerializeField] float startDeg = 0;

        bool ITransformConvertable.IsGroup => isGroup;

        public void ConvertItem(ItemBase item, float option, float time)
        {
            if (useDefault)
            {
                option = 1;
            }
            var addX = option * amp * Mathf.Sin(time * (2f / frequency.Time) * Mathf.PI + startDeg * Mathf.Deg2Rad);
            var pos = item.GetPos() + new Vector3(addX, 0);
            item.SetPos(pos);

            if (item is HoldNote hold)
            {
                var maskPos = hold.GetMaskPos() + new Vector2(addX, 0);
                hold.SetMaskPos(maskPos);
            }
        }
    }
}