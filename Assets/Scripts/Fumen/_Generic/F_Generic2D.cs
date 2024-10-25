using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public enum CreateNoteType
    {
        [InspectorName("なし")] _None,
        [InspectorName("タップ")] Normal,
        [InspectorName("スライド")] Slide,
        [InspectorName("フリック")] Flick,
        [InspectorName("ホールド")] Hold,
    }

    public interface INoteData
    {
        public CreateNoteType Type { get; }
        public float X { get; }

        public float Wait { get; }

        public float Width { get; }

        public float Length { get; }
    }

    [Serializable]
    public struct NoteData : INoteData
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

    [AddTypeMenu("◆汎用 ジェネレータ2D", -2), System.Serializable]
    public class F_Generic2D : Generator_Common
    {
        [SerializeField] float speedRate = 1f;

        [SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = true;
        
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
                NoteBase_2D note = Helper.GetNote2D(type, parentTs);
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(width);
                }
                Vector3 startPos = new(Inv(x), StartBase);
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(note).Forget();
                }
                else
                {
                    DropAsync(note, startPos).Forget();
                }

                float expectTime = startPos.y/Speed - Delta;
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
                        note.SetPos(new Vector3(Inv(x), StartBase) + time * vec);
                        await Helper.Yield();
                    }
                }
            }

            void MyHold(float x, float length, float width, Transform parentTs)
            {
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                Vector3 startPos = new (Inv(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(hold).Forget();
                }
                else
                {
                    DropAsync(hold, startPos).Forget();
                }

                float expectTime = startPos.y/Speed - Delta;
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
                    while (hold.IsActive && time < 8f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = Speed * Vector3.down;
                        hold.SetLength(holdTime * Speed);
                        hold.SetPos(new Vector3(Inv(x), StartBase, -0.04f) + time * vec);
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
            return noteDatas.Length + GetInverseSummary();
        }

        public override void OnSelect()
        {
            Preview();
        }
        public override void Preview()
        {
            MyUtility.DebugPreview2DNotes(noteDatas.Select(d => (INoteData)d), Helper, IsInverse);
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(speedRate, isSpeedChangable, isCheckSimultaneous);
            set
            {
                var texts = value.Split('|');

                speedRate = float.Parse(texts[0]);
                isSpeedChangable = bool.Parse(texts[1]);
                isCheckSimultaneous = bool.Parse(texts[2]);
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
