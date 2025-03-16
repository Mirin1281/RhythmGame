using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    public abstract class NoteCreateBase<T> : CommandBase where T : struct, INoteData
    {
        [SerializeField] protected Mirror mirror;
        [SerializeField] protected float speedRate = 1f;

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


            void CreateNote(in T noteData)
            {
                var type = noteData.Type;

                float lifeTime = MoveTime + 0.5f;
                if (type == RegularNoteType.Hold)
                {
                    lifeTime += noteData.Length.Time;
                }

                if (type is RegularNoteType.Normal or RegularNoteType.Slide)
                {
                    RegularNote note = Helper.GetRegularNote(type);
                    Move(note, noteData, lifeTime);
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
                    Move(hold, noteData, lifeTime);
                    AddExpect(hold, length: noteData.Length);
                }
            }
        }

        protected virtual void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, NoteJudgeStatus.ExpectType expectType = NoteJudgeStatus.ExpectType.Y_Static)
        {
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, pos, MoveTime - Delta, length, expectType));
        }

        protected void CreateDropNote(RegularNote note, NoteData data, TransformConverter transformConverter)
        {
            if (transformConverter.IsEmpty)
            {
                Helper.NoteInput.AddExpect(note, MoveTime - Delta, data.Length);
            }
            else
            {
                var basePos = new Vector3(mirror.Conv(data.X), 0);
                var pos = transformConverter.ConvertTransform(basePos, data.Option1, MoveTime).pos;
                var judgeStatus = new NoteJudgeStatus(note, pos, MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static);
                Helper.NoteInput.AddExpect(judgeStatus);
            }

            float lifeTime = MoveTime + 0.5f;
            if (data.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            if (HoldNote.TryParse(note, out var hold))
            {
                WhileYield(lifeTime, t =>
                {
                    if (hold.IsActive == false) return;
                    var basePos = new Vector3(mirror.Conv(data.X), (MoveTime - t) * Speed);
                    if (transformConverter.IsEmpty)
                    {
                        hold.SetPos(basePos);
                    }
                    else
                    {
                        var (pos, rot) = transformConverter.ConvertTransform(basePos, data.Option1, Time);
                        hold.SetPos(pos);
                        hold.SetMaskPos(MyUtility.GetRotatedPos(new Vector2(pos.x, 0), rot));
                        hold.SetRot(rot);
                    }
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    var basePos = new Vector3(mirror.Conv(data.X), (MoveTime - t) * Speed);
                    if (transformConverter.IsEmpty)
                    {
                        note.SetPos(basePos);
                    }
                    else
                    {
                        var (pos, rot) = transformConverter.ConvertTransform(basePos, data.Option1, Time);
                        note.SetPos(pos);
                        note.SetRot(rot);
                    }
                });
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
