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
        [SerializeField] float amp = 0.15f;
        [SerializeField] Lpb frequency = new Lpb(4);
        [SerializeField] float startDeg = 0;
        [SerializeField] bool shakeY = false;

        bool ITransformConvertable.IsGroup => isGroup;

        void ITransformConvertable.ConvertNote(RegularNote note, float option, float time)
        {
            var tmp = (2 * (time - RhythmGameManager.Offset) / frequency.Time + startDeg * Mathf.Deg2Rad) % 2 - 1;
            tmp = Mathf.Sqrt(1 - tmp * tmp);
            var addX = amp * Mathf.Abs(tmp) * option;
            var addPos = shakeY ? new Vector3(addX, addX) : new Vector3(addX, 0);

            var pos = note.GetPos() + addPos;
            note.SetPos(pos);

            if (note is HoldNote hold)
            {
                var maskPos = hold.GetMaskPos() + (Vector2)addPos;
                hold.SetMaskPos(maskPos);
            }
        }
    }
}
