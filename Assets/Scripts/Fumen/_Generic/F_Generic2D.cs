using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoteGenerating
{
    [AddTypeMenu("◆汎用ジェネレータ2D"), System.Serializable]
    public class F_Generic2D : Generator_Common
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
        }

        [SerializeField] float speedRate = 1f;

        [SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = false;
        
        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTask GenerateAsync()
        {
            int simultaneousCount = 0;
            float beforeTime = -1;
            NoteBase_2D beforeNote = null;

            var parentTs = CreateParent(parentGeneratable);

            foreach(var data in noteDatas)
            {
                var type = data.Type;
                if(type == CreateNoteType.Normal)
                {
                    MyNote(data.X, NoteType.Normal, data.Width, parentTs);
                }
                else if(type == CreateNoteType.Slide)
                {
                    MyNote(data.X, NoteType.Slide, data.Width, parentTs);
                }
                else if(type == CreateNoteType.Flick)
                {
                    MyNote(data.X, NoteType.Flick, data.Width, parentTs);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    MyHold(data.X, data.Length, data.Width, parentTs);
                }
                await Wait(data.Wait);
            }


            void MyNote(float x, NoteType type, float width, Transform parentTs)
            {
                NoteBase_2D note = Helper.GetNote2D(type);
                if(parentTs != null)
                {
                    note.transform.SetParent(parentTs);
                    note.transform.localRotation = default;
                }
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(width);
                }
                Vector3 startPos = new (ConvertIfInverse(x), StartBase);
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(note).Forget();
                }
                else
                {
                    DropAsync(note, startPos).Forget();
                }

                float distance = startPos.y - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, expectTime, isCheckSimultaneous: isCheckSimultaneous);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime,
                        isCheckSimultaneous: isCheckSimultaneous, mode: NoteExpect.ExpectMode.Y_Static);
                }
                SetSimultaneous(note, expectTime);


                async UniTask DropAsync_SpeedChangable(NoteBase_2D note)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = Speed * Vector3.down;
                        note.SetPos(new Vector3(ConvertIfInverse(x), StartBase) + time * vec);
                        await Helper.Yield();
                    }
                }
            }

            void MyHold(float x, float length, float width, Transform parentTs)
            {
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed);
                if(parentTs != null)
                {
                    hold.transform.SetParent(parentTs);
                    hold.transform.localRotation = default;
                }
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                Vector3 startPos = new (ConvertIfInverse(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(hold).Forget();
                }
                else
                {
                    DropAsync(hold, startPos).Forget();
                }

                float distance = startPos.y - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(hold, expectTime, holdTime, isCheckSimultaneous);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime,
                        holdTime, isCheckSimultaneous: isCheckSimultaneous, mode: NoteExpect.ExpectMode.Y_Static);
                }
                SetSimultaneous(hold, expectTime);


                async UniTask DropAsync_SpeedChangable(HoldNote hold)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (hold.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = Speed * Vector3.down;
                        hold.SetLength(holdTime * Speed);
                        hold.SetPos(new Vector3(ConvertIfInverse(x), StartBase, -0.04f) + time * vec);
                        await Helper.Yield();
                    }
                }
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
                (byte)Mathf.Clamp(246 - noteDatas.Length * 2, 96, 246),
                (byte)Mathf.Clamp(230 - noteDatas.Length * 2, 130, 230),
                255);
        }

        protected override string GetSummary()
        {
            string invText = IsInverse ? "<color=#0000ff><b>(inv)</b></color>" : string.Empty;
            return $"{noteDatas.Length}  {invText}";
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
                var line = Helper.PoolManager.LinePool.GetLine();
                line.SetPos(new Vector3(0, lineY));
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
                var startPos = new Vector3(ConvertIfInverse(x), y);
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
                hold.SetMaskLocalPos(new Vector2(ConvertIfInverse(x), 0));
                var startPos = new Vector3(ConvertIfInverse(x), y);
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
            get => MyUtility.GetContentFrom(IsInverse, speedRate, isSpeedChangable, isCheckSimultaneous);
            set
            {
                var texts = value.Split('|');

                IsInverse = bool.Parse(texts[0]);
                speedRate = float.Parse(texts[1]);
                isSpeedChangable = bool.Parse(texts[2]);
                isCheckSimultaneous = bool.Parse(texts[3]);
            }
        }

        public override string CSVContent2
        {
            get => MyUtility.GetContentFrom(noteDatas);
            set => noteDatas = MyUtility.GetArrayFrom<NoteData>(value);
        }

        public override string CSVContent3
        {
            get => parentGeneratable?.GetContent();
            set => parentGeneratable ??= ParentGeneratorBase.CreateFrom(value);
        }
    }
}
