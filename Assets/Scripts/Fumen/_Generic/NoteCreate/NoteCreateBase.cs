using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public abstract class NoteCreateBase<T> : CommandBase where T : struct, INoteData
    {
        [SerializeField] protected Mirror mirror;
        [SerializeField] float speedRate = 1f;

        /// <summary>
        /// NoteData内のWaitの加算
        /// </summary>
        protected Lpb WaitDelta { get; private set; }

        protected override float Speed => base.Speed * speedRate;

        protected abstract T[] NoteDatas { get; }
        protected abstract void Move(RegularNote note, T data);

        protected override async UniTaskVoid ExecuteAsync()
        {
            WaitDelta = default;
            foreach (var data in NoteDatas)
            {
                await Wait(data.Wait);
                WaitDelta += data.Wait;
                CreateNote(data);
            }


            void CreateNote(in T noteData)
            {
                var type = noteData.Type;
                if (type is RegularNoteType.Normal or RegularNoteType.Slide)
                {
                    RegularNote note = Helper.GetRegularNote(type);
                    Move(note, noteData);
                    AddExpect(note);
                }
                else if (type is RegularNoteType.Hold)
                {
                    if (noteData.Length == default)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    HoldNote hold = Helper.GetHold(noteData.Length * Speed);
                    hold.SetMaskPos(new Vector2(mirror.Conv(noteData.X), 0));
                    Move(hold, noteData);
                    AddExpect(hold, length: noteData.Length);
                }
            }
        }

        protected virtual void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, NoteJudgeStatus.ExpectType expectType = NoteJudgeStatus.ExpectType.Y_Static)
        {
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, pos, MoveTime - Delta, length, expectType));
        }


#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.GetNoteCommandColor();
        }

        protected override string GetSummary()
        {
            return NoteDatas?.Length + mirror.GetStatusText();
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.Delay);
        }
        public override void OnPeriod()
        {
            DebugPreview(true, delay: new Lpb(4));
        }

        void DebugPreview(bool beforeClear, Lpb delay)
        {
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            previewer.DebugPreview2DNotes(NoteDatas, Helper.PoolManager, mirror, beforeClear, delay);
        }
#endif
    }
}
