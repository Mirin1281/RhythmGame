using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using System.Text;

namespace NoteGenerating
{
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
            [SerializeField, Min(0)] float width;
            [SerializeField, Min(0)] float length;

            public readonly CreateNoteType Type => type;
            public readonly float X => x;
            public readonly float Wait => wait;
            public readonly float Width => width;
            public readonly float Length => length;

            public NoteData(CreateNoteType type, float x, float wait, float width, float length)
            {
                this.type = type;
                this.x = x;
                this.wait = wait;
                this.width = width;
                this.length = length;
            }
        }

        [SerializeField] float speedRate = 1f;
        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = false;
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
                    MyNote(data.X, NoteType.Normal, data.Width);
                }
                else if(type == CreateNoteType.Slide)
                {
                    MyNote(data.X, NoteType.Slide, data.Width);
                }
                else if(type == CreateNoteType.Flick)
                {
                    MyNote(data.X, NoteType.Flick, data.Width);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    MyHold(data.X, data.Length, data.Width);
                }
                await Wait(data.Wait);
            }


            void MyNote(float x, NoteType type, float width)
            {
                NoteBase_2D note = Helper.GetNote2D(type);
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(width);
                }
                Vector3 startPos = new (Inverse(x), StartBase);
                DropAsync(note, startPos).Forget();

                float distance = startPos.y - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                Helper.NoteInput.AddExpect(note, startPos.x, expectTime, isCheckSimultaneous: isCheckSimultaneous);

                SetSimultaneous(note, expectTime);
            }

            void MyHold(float x, float length, float width)
            {
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed);
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                Vector3 startPos = new (Inverse(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                DropAsync(hold, startPos).Forget();

                float distance = startPos.y - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                Helper.NoteInput.AddExpect(hold, startPos.x, expectTime, holdTime, isCheckSimultaneous: isCheckSimultaneous);

                SetSimultaneous(hold, expectTime);
            }

            // 同時押しをこのコマンド内でのみチェックします。
            // NoteInput内でするよりも軽量なのでデフォルトではこちらを使用します
            void SetSimultaneous(NoteBase_2D note, float expectTime)
            {
                // NoteInput内で行う場合は不要
                if(isCheckSimultaneous) return;

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
            }
        }

        protected override string GetName()
        {
            return "汎用2D";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(
                255,
                (byte)Mathf.Clamp(246 - noteDatas.Length, 96, 246),
                (byte)Mathf.Clamp(230 - noteDatas.Length, 130, 230),
                255);
        }

        protected override string GetSummary()
        {
            return $"{noteDatas.Length}  {GetInverseSummary()}";
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
                    DebugNote(data.X, y, NoteType.Normal, data.Width);
                }
                else if(type == CreateNoteType.Slide)
                {
                    DebugNote(data.X, y, NoteType.Slide, data.Width);
                }
                else if(type == CreateNoteType.Flick)
                {
                    DebugNote(data.X, y, NoteType.Flick, data.Width);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    DebugHold(data.X, y, data.Length, data.Width);
                }
                y += Helper.GetTimeInterval(data.Wait) * Speed;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.SetPos(new Vector3(0, lineY));
                line.SetHeight(0.06f);
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * Speed;
                if(lineY > y) break;
            }


            void DebugNote(float x, float y, NoteType type, float width)
            {
                NoteBase_2D note = Helper.GetNote2D(type);
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(width);
                }
                var startPos = new Vector3(Inverse(x), y);
                note.SetPos(startPos);
                note.transform.SetParent(previewObj.transform);

                SetSimultaneous(note, y);
            }

            void DebugHold(float x, float y, float length, float width)
            {
                var holdTime = Helper.GetTimeInterval(length);
                var hold = Helper.GetHold(holdTime * Speed);
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                hold.SetMaskLocalPos(new Vector2(Inverse(x), 0));
                var startPos = new Vector3(Inverse(x), y);
                hold.SetPos(startPos);
                hold.transform.SetParent(previewObj.transform);

                SetSimultaneous(hold, y);
            }

            void SetSimultaneous(NoteBase_2D note, float y)
            {
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
        }

        public override string CSVContent1
        {
            get => IsInverse + "|" + speedRate + "|" + isCheckSimultaneous;
            set
            {
                var texts = value.Split('|');
                IsInverse = bool.Parse(texts[0]);
                speedRate = float.Parse(texts[1]);
                isCheckSimultaneous = bool.Parse(texts[2]);
            }
        }

        public override string CSVContent2
        {
            get => MyUtility.GetArrayContent(noteDatas);
            set => noteDatas = MyUtility.GetArrayFromContent<NoteData>(value);
        }
    }
}
