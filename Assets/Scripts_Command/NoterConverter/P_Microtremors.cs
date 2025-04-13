using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "P_Curve/P_Distortion")]
    [AddTypeMenu("微細な揺れ", -60), System.Serializable]
    public class P_Microtremors : ITransformConvertable
    {
        [SerializeField] bool useDefault;
        [Header("オプション1 : 揺れの係数 デフォルト1")]
        [SerializeField] bool isPosDistortion = true;
        [SerializeField] bool isRotDistortion;
        [SerializeField] float frequency = 3f;
        [SerializeField] float posAmp = 0.3f;
        [SerializeField] float rotAmp = 5;
        [SerializeField] int baseSeed = 774932;

        bool ITransformConvertable.IsGroup => false;

        void ITransformConvertable.ConvertItem(ItemBase item, float option, float time)
        {
            if (useDefault)
                option = 1;
            // new Unity.Mathematics.Random((uint)(baseSeed + note.GetHashCode()));
            var random = Unity.Mathematics.Random.CreateFromIndex((uint)(baseSeed + item.GetHashCode()));
            float randFrequency = random.NextFloat(-frequency, frequency);
            float randPhase = random.NextFloat(0, 2 * Mathf.PI);

            (Vector3 pos, float rot) moveFunc(float t)
            {
                float addX = 0;
                if (isPosDistortion)
                {
                    addX = posAmp * option * Mathf.Sin(t * randFrequency + randPhase);
                }
                Vector3 pos = new Vector3(addX, 0);

                float rot = 0;
                if (isRotDistortion)
                {
                    rot = rotAmp * option * Mathf.Sin(t * randFrequency + randPhase);
                }
                return (pos, rot);
            }

            var ts = moveFunc(time);
            item.SetPosAndRot(ts.pos + item.GetPos(), ts.rot);
            if (item is HoldNote hold)
            {
                //var maskPos = hold.GetMaskPos();
                //hold.SetMaskPos(MyUtility.GetRotatedPos(maskPos + new Vector2(ts.pos.x, 0), ts.rot));
                hold.SetMaskRot(0);
            }
        }
    }
}
