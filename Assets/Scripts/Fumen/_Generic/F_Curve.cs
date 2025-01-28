using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_2D_Curve")]
    [AddTypeMenu("◆ノーツ生成 曲がって落下", -60), System.Serializable]
    public class F_Curve : CommandBase
    {
        [Serializable]
        public struct NoteData : INoteData
        {
            [SerializeField, Min(0)] float wait;
            [SerializeField] RegularNoteType type;
            [SerializeField] float x;
            [SerializeField] bool isCurveReverse;
            [SerializeField, Min(0)] float length;

            public readonly float Wait => wait;
            public readonly RegularNoteType Type => type;
            public readonly float X => x;
            public readonly bool IsCurveReverse => isCurveReverse;
            public readonly float Length => length;
        }

        [SerializeField] Mirror mirror;

        [SerializeField] float speedRate = 1f;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentCreatable parentCreatable;

        [SerializeField]
        float radius = 14;

        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTask ExecuteAsync()
        {
            var parentTs = parentCreatable?.CreateParent(Delta, Helper, mirror);

            foreach (var data in noteDatas)
            {
                await Wait(data.Wait);
                var type = data.Type;
                if (type is RegularNoteType.Normal or RegularNoteType.Slide or RegularNoteType.Flick)
                {
                    MyNote(data.X, type, data.IsCurveReverse, radius, parentTs);
                }
                else if (type == RegularNoteType.Hold)
                {
                    if (data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    MyHold(data.X, data.Length, data.IsCurveReverse, radius, parentTs);
                }
            }


            RegularNote MyNote(float x, RegularNoteType type, bool isCurveinverse = false, float radius = 10, Transform parentTs = null)
            {
                int a = (x > 0 ^ isCurveinverse) ? -1 : 1;
                RegularNote note = Helper.GetNote(type, parentTs);
                float moveTime = StartBase / Speed;
                CurveAsync(note, moveTime).Forget();

                float expectTime = StartBase / Speed - Delta;
                if (parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, expectTime);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = mirror.Conv(x) * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, expectType: NoteJudgeStatus.ExpectType.Y_Static);
                }
                return note;


                async UniTask CurveAsync(RegularNote note, float moveTime)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius * a + Mathf.PI * (a == 1 ? 0 : 1);
                        note.SetPos(new Vector3(mirror.Conv(x) - a * radius, 0) + radius * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                        note.SetRot(dir * Mathf.Rad2Deg);
                        await Helper.Yield();
                    }
                }
            }

            void MyHold(float x, float length, bool isCurveinverse = false, float radius = 10, Transform parentTs = null)
            {
                Debug.LogWarning("未実装");
                isCurveinverse = x > 0 ^ isCurveinverse;
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
                Vector3 startPos = new(mirror.Conv(x), StartBase);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                float moveTime = StartBase / Speed;
                CurveAsync(hold, moveTime).Forget();

                float expectTime = startPos.y / Speed - Delta;
                if (parentTs == null)
                {
                    Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime,
                        holdTime, isCheckSimultaneous: true, expectType: NoteJudgeStatus.ExpectType.Y_Static);
                }


                async UniTask CurveAsync(RegularNote note, float moveTime)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius * (isCurveinverse ? -1 : 1);
                        note.SetPos(new Vector3(x - radius, 0) + radius * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                        note.SetRot(dir * Mathf.Rad2Deg);
                        await Helper.Yield();
                    }
                }
            }
        }

#if UNITY_EDITOR

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
            return noteDatas.Length + mirror.GetStatusText();
        }

        public override string CSVContent1
        {
            get => parentCreatable?.GetContent();
            set => parentCreatable ??= ParentCreatorBase.CreateFrom(value);
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.BeatDelta);
        }
        public override void OnPeriod()
        {
            DebugPreview(false);
        }

        void DebugPreview(bool beforeClear = true, int beatDelta = 1)
        {
            FumenDebugUtility.DebugPreview2DNotes(noteDatas, Helper, mirror, beforeClear, beatDelta);
        }
#endif
    }
}
