using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆汎用ジェネレータ"), System.Serializable]
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

            public NoteData(CreateNoteType type, float x, float wait, float length)
            {
                this.type = type;
                this.x = x;
                this.wait = wait;
                this.length = length;
            }
        }

        [SerializeField] string summary;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];
        
        protected override async UniTask GenerateAsync()
        {
            foreach(var data in noteDatas)
            {
                var type = data.Type;
                if(type == CreateNoteType.Normal)
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
                        continue;
                    }
                    Hold(data.X, data.Length);
                }
                await Wait(data.Wait);
            }
        }

        protected override string GetName()
        {
            return "汎用";
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.VersatileCommandColor;
        }

        protected override string GetSummary()
        {
            string s = noteDatas.Length.ToString();
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            return $"{s} : {summary}{GetInverseSummary()}";
        }

        public override void OnSelect()
        {
            Preview();
        }

        public override void Preview()
        {
            GameObject previewObj = MyUtility.GetPreviewObject();
            float y = 0f;
            foreach(var data in noteDatas)
            {
                var type = data.Type;
                if(type == CreateNoteType.Normal)
                {
                    Note(data.X, y, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    Note(data.X, y, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    Note(data.X, y, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    Hold(data.X, y, data.Length);
                }
                y += GetTimeInterval(data.Wait) * Speed;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineY += GetTimeInterval(4) * Speed;
                if(lineY > y) break;
            }

            void Note(float x, float y, NoteType type)
            {
                NoteBase note = Helper.GetNote(type);
                var startPos = new Vector3(Inverse(x), y);
                note.SetPos(startPos);
                note.transform.parent = previewObj.transform;
            }

            void Hold(float x, float y, float length)
            {
                var hold = Helper.GetHold();
                var holdTime = GetTimeInterval(length);
                hold.SetLength(holdTime * Speed);
                hold.SetMaskLocalPos(new Vector2(Inverse(x), 0));
                var startPos = new Vector3(Inverse(x), y);
                hold.SetPos(startPos);
                hold.transform.parent = previewObj.transform;
            }

            float GetTimeInterval(float lpb)
            {
                if(lpb == 0) return 0;
                return 240f / RhythmGameManager.DebugBpm / lpb;
            }
        }

        public override string CSVContent1
        {
            get
            {
                string text = string.Empty;
                text += IsInverse + "\n";
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    var data = noteDatas[i];
                    text += data.Type + " ";
                    text += data.X + " ";
                    text += data.Wait + " ";
                    text += data.Length;
                    if(i == noteDatas.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var dataTexts = value.Split("\n");
                SetInverse(bool.Parse(dataTexts[0]));
                var noteDatas = new NoteData[dataTexts.Length - 1];
                for(int i = 0; i < dataTexts.Length - 1; i++)
                {
                    var contents = dataTexts[i + 1].Split(' ');
                    noteDatas[i] = new NoteData(
                        Enum.Parse<CreateNoteType>(contents[0]),
                        float.Parse(contents[1]),
                        float.Parse(contents[2]),
                        float.Parse(contents[3]));
                }
                this.noteDatas = noteDatas;
            }
        }
    }
}
