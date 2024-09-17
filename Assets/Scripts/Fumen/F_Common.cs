using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Common")]
    [AddTypeMenu("◆汎用ジェネレータ2D"), System.Serializable]
    public class F_Generic2D : Generator_2D
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
        [SerializeField] float speedRate = 1f;
        [SerializeField] bool isCheckSimultaneous = false;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTask GenerateAsync()
        {
            int simultaneousCount = 0;
            float beforeTime = -1;
            NoteBase_2D beforeNote = null;

            foreach(var data in noteDatas)
            {
                var type = data.Type;
                if(type == CreateNoteType.Normal)
                {
                    MyNote(data.X, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    MyNote(data.X, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    MyNote(data.X, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    MyHold(data.X, data.Length);
                }
                await Wait(data.Wait);
            }


            NoteBase_2D MyNote(float x, NoteType type, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
                Vector3 startPos = new Vector3(Inverse(x), StartBase);
                DropAsync(note, startPos, delta).Forget();

                float distance = startPos.y - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                NoteExpect expect = new NoteExpect(note, new Vector2(startPos.x, 0), expectTime);
                Helper.NoteInput.AddExpect(expect, isCheckSimultaneous);

                if(beforeTime == expectTime)
                {
                    if(simultaneousCount == 1)
                    {
                        Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    Helper.PoolManager.SetSimultaneousSprite(note);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeTime = expectTime;
                beforeNote = note;
                
                return note;
            }

            HoldNote MyHold(float x, float length, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                HoldNote hold = Helper.GetHold();
                float holdTime = Helper.GetTimeInterval(length);
                hold.SetLength(holdTime * Speed);
                Vector3 startPos = new Vector3(Inverse(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                DropAsync(hold, startPos, delta).Forget();

                float distance = startPos.y - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                float holdEndTime = expectTime + holdTime;
                NoteExpect expect = new NoteExpect(hold, new Vector2(startPos.x, 0), expectTime, holdEndTime: holdEndTime);
                Helper.NoteInput.AddExpect(expect, isCheckSimultaneous);

                if(beforeTime == expectTime)
                {
                    if(simultaneousCount == 1)
                    {
                        Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    Helper.PoolManager.SetSimultaneousSprite(hold);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeTime = expectTime;
                beforeNote = hold;

                return hold;
            }
        }

        protected override string GetName()
        {
            return "汎用2D";
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
            int simultaneousCount = 0;
            float beforeY = -1;
            NoteBase_2D beforeNote = null;
            float y = 0f;
            foreach(var data in noteDatas)
            {
                var type = data.Type;
                if(type == CreateNoteType.Normal)
                {
                    DebugNote(data.X, y, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    DebugNote(data.X, y, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    DebugNote(data.X, y, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    DebugHold(data.X, y, data.Length);
                }
                y += Helper.GetTimeInterval(data.Wait) * Speed;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineY += Helper.GetTimeInterval(4) * Speed;
                if(lineY > y) break;
            }

            void DebugNote(float x, float y, NoteType type)
            {
                NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
                var startPos = new Vector3(Inverse(x), y);
                note.SetPos(startPos);
                note.transform.parent = previewObj.transform;

                if(beforeY == y)
                {
                    if(simultaneousCount == 1)
                    {
                        Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    Helper.PoolManager.SetSimultaneousSprite(note);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeY = y;
                beforeNote = note;
            }

            void DebugHold(float x, float y, float length)
            {
                var hold = Helper.GetHold();
                var holdTime = Helper.GetTimeInterval(length);
                hold.SetLength(holdTime * Speed);
                hold.SetMaskLocalPos(new Vector2(Inverse(x), 0));
                var startPos = new Vector3(Inverse(x), y);
                hold.SetPos(startPos);
                hold.transform.parent = previewObj.transform;

                if(beforeY == y)
                {
                    if(simultaneousCount == 1)
                    {
                        Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    Helper.PoolManager.SetSimultaneousSprite(hold);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeY = y;
                beforeNote = hold;
            }

        }

        public override string CSVContent1
        {
            get => speedRate + " " + isCheckSimultaneous;
            set
            {
                var texts = value.Split(' ');
                speedRate = float.Parse(texts[0]);
                isCheckSimultaneous = bool.Parse(texts[1]);
            }
        }

        public override string CSVContent2
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
                var texts = value.Split("\n");
                SetInverse(bool.Parse(texts[0]));
                var noteDatas = new NoteData[texts.Length - 1];
                for(int i = 0; i < texts.Length - 1; i++)
                {
                    var contents = texts[i + 1].Split(' ');
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
