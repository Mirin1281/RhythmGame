using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆汎用 3Dに2Dノーツを流す", -1), System.Serializable]
    public class F_3DGeneric : Generator_Common
    {
        [SerializeField] float speedRate = 1f;

        [SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = false;
        
        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        protected override float Speed3D => base.Speed3D * speedRate;

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
                note.SetRotate(new Vector3(90, 0, 0));
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(1.4f * width);
                }
                else
                {
                    note.SetWidth(1.4f);
                }
                note.SetHeight(5f);
                Vector3 startPos = new Vector3(Inv(x), 0.04f, StartBase3D);
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(note).Forget();
                }
                else
                {
                    DropAsync3D(note, startPos).Forget();
                }

                float expectTime = startPos.z/Speed3D - Delta;
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
                        var vec = Speed3D * Vector3.back;
                        note.SetPos(new Vector3(Inv(x), StartBase3D) + time * vec);
                        await Helper.Yield();
                    }
                }
            }

            void MyHold(float x, float length, float width, Transform parentTs)
            {
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed3D, parentTs);
                hold.SetRotate(new Vector3(90, 0, 0));
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(1.4f * width);
                }
                else
                {
                    hold.SetWidth(1.4f);
                }
                Vector3 startPos = new Vector3(Inv(x), 0.04f, StartBase3D);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(hold).Forget();
                }
                else
                {
                    DropAsync3D(hold, startPos).Forget();
                }

                float expectTime = startPos.z/Speed3D - Delta;
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
                        var vec = Speed3D * Vector3.down;
                        hold.SetLength(holdTime * Speed3D);
                        hold.SetPos(new Vector3(Inv(x), StartBase3D, -0.04f) + time * vec);
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
            return "3Dレーン 2D";
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

        public override void OnSelect(bool isFirst)
        {
            DebugPreview(isFirst);
        }
        public override void Preview()
        {
            DebugPreview(false);
        }

        void DebugPreview(bool isClearPreview)
        {
            GameObject previewObj = MyUtility.GetPreviewObject(isClearPreview);
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
                var startPos = new Vector3(Inv(x), y);
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
                hold.SetMaskLocalPos(new Vector2(Inv(x), 0));
                var startPos = new Vector3(Inv(x), y);
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
            get => MyUtility.GetContent(noteDatas);
            set => noteDatas = MyUtility.GetArrayFrom<NoteData>(value);
        }

        public override string CSVContent3
        {
            get => parentGeneratable?.GetContent();
            set => parentGeneratable ??= ParentGeneratorBase.CreateFrom(value);
        }
    }
}
