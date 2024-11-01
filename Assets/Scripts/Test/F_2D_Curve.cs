using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆2D 曲がって落下", -2), System.Serializable]
    public class F_2D_Curve : Generator_Common
    {
        [Serializable]
        public struct NoteData : INoteData
        {
            [SerializeField] CreateNoteType type;
            [SerializeField] float x;
            [SerializeField, Min(0)] float wait;
            [SerializeField, Min(0)] float width;
            [SerializeField] bool isCurveInverse;
            [SerializeField, Min(0)] float length;

            public readonly CreateNoteType Type => type;
            public readonly float X => x;
            public readonly float Wait => wait;
            public readonly float Width => width;
            public readonly bool IsCurveInverse => isCurveInverse;
            public readonly float Length => length;
        }

        [SerializeField] float speedRate = 1f;

        //[SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        [SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        bool isCheckSimultaneous = true;

        [SerializeField]
        float radius = 14;
        
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
                    MyNoteCircle(data.X, NoteType.Normal, data.Width, data.IsCurveInverse, radius, parentTs);
                }
                else if(type == CreateNoteType.Slide)
                {
                    MyNoteCircle(data.X, NoteType.Slide, data.Width, data.IsCurveInverse, radius, parentTs);
                }
                else if(type == CreateNoteType.Flick)
                {
                    MyNoteCircle(data.X, NoteType.Flick, data.Width, data.IsCurveInverse, radius, parentTs);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    MyHold(data.X, data.Length, data.Width, data.IsCurveInverse, radius, parentTs);
                }
                await Wait(data.Wait);
            }


            NoteBase_2D MyNoteCircle(float x, NoteType type, float width, bool isCurveinverse = false, float radius = 10, Transform parentTs = null)
            {
                int a = x > 0 ^ isCurveinverse ? -1 : 1;
                NoteBase_2D note = Helper.GetNote2D(type, parentTs);
                if((width is 0 or 1) == false)
                {
                    note.SetWidth(width);
                }
                float moveTime = StartBase / Speed;
                CurveAsync(note, moveTime).Forget();

                float expectTime = StartBase / Speed - Delta;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, expectTime);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = Inv(x) * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
                }
                return note;


                async UniTask CurveAsync(NoteBase note, float moveTime)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius * a + Mathf.PI * (a == 1 ? 0 : 1);
                        note.SetPos(new Vector3(Inv(x) - a * radius, 0) + radius * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                        note.SetRotate(dir * Mathf.Rad2Deg);
                        await Helper.Yield();
                    }
                }
            }

            void MyHold(float x, float length, float width, bool isCurveinverse = false, float radius = 10, Transform parentTs = null)
            {
                Debug.LogWarning("未実装");
                isCurveinverse = x > 0 ^ isCurveinverse;
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
                if((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                Vector3 startPos = new (Inv(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                float moveTime = StartBase / Speed;
                CurveAsync(hold, moveTime).Forget();

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


                async UniTask CurveAsync(NoteBase note, float moveTime)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius * (isCurveinverse ? -1 : 1);
                        note.SetPos(new Vector3(x - radius, 0) + radius * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                        note.SetRotate(dir * Mathf.Rad2Deg);
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
            return "カーブ2D";
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
            MyUtility.DebugPreview2DNotes(
                noteDatas.Select(d => (INoteData)d), Helper, IsInverse, isClearPreview);
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(speedRate,  isCheckSimultaneous, radius);
            set
            {
                var texts = value.Split('|');

                speedRate = float.Parse(texts[0]);
                isCheckSimultaneous = bool.Parse(texts[1]);
                radius = float.Parse(texts[2]);
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
