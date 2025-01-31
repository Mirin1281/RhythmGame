using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public interface INoteData
    {
        public RegularNoteType Type { get; }
        public float X { get; }
        public float Wait { get; }
        public float Length { get; }
    }

    [Serializable]
    public struct NoteData : INoteData
    {
        [SerializeField, Min(0)] float wait;
        [SerializeField] RegularNoteType type;
        [SerializeField] float x;
        [SerializeField, Min(0)] float length;

        public readonly float Wait => wait;
        public readonly RegularNoteType Type => type;
        public readonly float X => x;
        public readonly float Length => length;
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Generic2D")]
    [AddTypeMenu("◆一般ノーツ生成", -100), System.Serializable]
    public class F_General : CommandBase, IFieldAddHandler, IFieldDeleteHandler
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentCreatable parentCreatable;

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
                    Note(data.X, data.Type, parentTs: parentTs);
                }
                else if (type is RegularNoteType.Hold)
                {
                    if (data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    Hold(data.X, data.Length, parentTs: parentTs);
                }
            }
        }

        RegularNote Note(float x, RegularNoteType type, Transform parentTs = null)
        {
            RegularNote note = Helper.GetRegularNote(type, parentTs);
            Vector3 startPos = new(mirror.Conv(x), StartBase);
            DropAsync(note, startPos, Delta).Forget();

            // 現在の時間から何秒後に着弾するか
            float expectTime = startPos.y / Speed - Delta;
            if (parentTs == null)
            {
                Helper.NoteInput.AddExpect(note, expectTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, expectType: NoteJudgeStatus.ExpectType.Y_Static);
            }
            return note;
        }

        HoldNote Hold(float x, float length, Transform parentTs = null)
        {
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
            Vector3 startPos = mirror.Conv(new Vector3(x, StartBase));
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            DropAsync(hold, startPos, Delta).Forget();

            float expectTime = startPos.y / Speed - Delta;
            if (parentTs == null)
            {
                Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, holdTime, expectType: NoteJudgeStatus.ExpectType.Y_Static);
            }
            return hold;
        }

#if UNITY_EDITOR

        //string[] IFieldChengeHandler.AddedFieldNames => new string[] { nameof(mirror), nameof(parentCreatable) };
        string[] IFieldAddHandler.AddedFieldNames => new string[] { };
        int[] IFieldDeleteHandler.DeletedFieldIndices => new int[] { };

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
            return noteDatas.Length + mirror.GetStatusText();
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.BeatDelta);
        }
        public override void OnPeriod()
        {
            DebugPreview();
        }

        void DebugPreview(bool beforeClear = true, int beatDelta = 1)
        {
            FumenDebugUtility.DebugPreview2DNotes(noteDatas, Helper, mirror, beforeClear, beatDelta);
        }
#endif
    }
}
