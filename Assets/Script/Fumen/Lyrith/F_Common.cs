using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/◆汎用ジェネレータ"), System.Serializable]
    public class F_Common : Generator_Type1
    {
        public enum CreateNoteType
        {
            [InspectorName("なし")] _None,
            [InspectorName("タップ")] Normal,
            [InspectorName("スライド")] Slide,
            [InspectorName("フリック")] Flick,
            [InspectorName("ホールド")] Hold,
        }

        [Serializable]
        public struct NoteData
        {
            [SerializeField] CreateNoteType type;
            [SerializeField] float x;
            [SerializeField, Min(0)] float wait;
            [SerializeField, Min(0)] float length;
            public readonly CreateNoteType Type => type;
            public readonly float X => x;
            public readonly float Wait => wait;
            public readonly float Length => length;
        }

        [SerializeField] string summary;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];
        
        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                Create(data);
                await Wait(data.Wait);
            }


            void Create(NoteData data)
            {
                var type = data.Type;
                if(type == CreateNoteType._None)
                {
                    return;
                }
                else if(type == CreateNoteType.Normal)
                {
                    Note(data.X, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    Note(data.X, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    Note(data.X, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    Hold(data.X, data.Length);
                }
            }
        }

        protected override string GetName()
        {
            return "汎用";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(255, 226, 200, 255);
        }

        protected override string GetSummary()
        {
            string s = string.Empty;
            if(noteDatas == null) return s;
            s += noteDatas.Length;
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            s += " : " + summary;
            return s + GetInverseSummary();
        }
    }
}
