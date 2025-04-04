using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Awakening 揺れるノーツ"), System.Serializable]
    public class P_Reverse_Transparent : ITransformConvertable
    {
        [Header("オプション: 揺れの係数(デフォルトは1)")]
        [SerializeField] bool isGroup = true;
        [SerializeField] float amp = 7;
        [SerializeField] Lpb frequency = new Lpb(0.25f);
        [SerializeField] float startDeg = 0;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
        {
            var startRad = startDeg * Mathf.Deg2Rad;
            var tmpX = Mathf.Sin(2f * Mathf.PI * time / frequency.Time + startRad);
            var addX = amp * Mathf.Abs(tmpX) * option;
            var addY = amp * Mathf.Abs(tmpX) * option;


            var pos = note.GetPos() + new Vector3(addX, addY);
            note.SetPos(pos);

            if (note is HoldNote hold)
            {
                var maskPos = hold.GetMaskPos() + new Vector2(addX, addY);
                hold.SetMaskPos(maskPos);
            }
        }
    }
}
