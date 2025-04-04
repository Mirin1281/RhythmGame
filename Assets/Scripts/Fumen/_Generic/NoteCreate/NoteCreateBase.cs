using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    public abstract class NoteCreateBase<T> : CommandBase where T : struct, INoteData
    {
        [SerializeField] protected Mirror mirror;
        [SerializeField] protected float speedRate = 1f;
        [SerializeField] bool isVerticalRange;
        float baseTime;

        /// <summary>
        /// コマンドがExecuteされてから経過した時間
        /// </summary>
        protected float Time => CurrentTime - baseTime;
        protected override float Speed => base.Speed * speedRate;

        protected abstract T[] NoteDatas { get; }
        protected abstract void Move(RegularNote note, T data, float lifeTime);

        protected override async UniTaskVoid ExecuteAsync()
        {
            baseTime = CurrentTime - Delta;
            foreach (var data in NoteDatas)
            {
                await Wait(data.Wait);
                CreateNote(data);
            }


            void CreateNote(T noteData)
            {
                var type = noteData.Type;

                float lifeTime = MoveTime + 0.2f;
                if (type == RegularNoteType.Hold)
                {
                    lifeTime += noteData.Length.Time;
                }

                if (Delta > lifeTime) return;

                if (type is RegularNoteType.Normal or RegularNoteType.Slide)
                {
                    RegularNote note = Helper.GetRegularNote(type);
                    note.IsVerticalRange = isVerticalRange;
                    Move(note, noteData, lifeTime);
                }
                else if (type is RegularNoteType.Hold)
                {
                    if (noteData.Length == default)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    HoldNote hold = Helper.GetHold(noteData.Length * Speed);
                    hold.IsVerticalRange = isVerticalRange;
                    hold.SetMaskPos(new Vector2(mirror.Conv(noteData.X), 0));
                    Move(hold, noteData, lifeTime);
                }
            }
        }


#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.GetNoteCommandColor(NoteDatas);
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
