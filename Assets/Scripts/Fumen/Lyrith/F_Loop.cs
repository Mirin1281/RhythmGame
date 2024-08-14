using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/◆汎用ループ"), System.Serializable]
    public class F_Loop : Generator_Type1
    {
        [Serializable]
        public struct NoteData
        {
            [SerializeField] float x;
            [SerializeField] bool isInvalid;
            public readonly float X => x;
            public readonly bool IsInvalid => isInvalid;
        }

        [SerializeField] string summary;
        [SerializeField] NoteType noteType;
        [SerializeField, Min(0)] float lpb = 4;
        [SerializeField, Min(0)] float length = 4;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        
        protected override async UniTask GenerateAsync()
        {
            if(noteType == NoteType._None)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if(noteType == NoteType.Hold)
            {
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    if(noteDatas[i].IsInvalid == false)
                    {
                        Hold(noteDatas[i].X, length);
                    }
                    await Wait(lpb);
                }
            }
            else
            {
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    if(noteDatas[i].IsInvalid == false)
                    {
                        Note(noteDatas[i].X, noteType, Delta);
                    }
                    await Wait(lpb);
                }
            }
        }

        protected override string GetName()
        {
            return "ループ";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(255, 226, 200, 255);
        }

        protected override string GetSummary()
        {
            string s = string.Empty;
            if(noteDatas == null) return s;
            s = $"<color=#dc143c>{noteType}</color>  LPB:{lpb}  Length:{noteDatas.Length}";
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            return $"{s} : {summary}{GetInverseSummary()}";
        }
    }
}
