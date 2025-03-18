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

        /// <summary>
        /// 設定した軌道、角度でノーツを移動させます
        /// </summary>
        protected void MoveNote(RegularNote note, NoteData data, TransformConverter transformConverter, Func<float, (Vector3 pos, float rot)> moveFunc, bool autoExpect = true)
        {
            if (autoExpect)
            {
                // 着弾地点を設定 //
                var (baseExpectPos, baseExpectRot) = moveFunc.Invoke(MoveTime);
                var (expectPos, _) = transformConverter.Convert(
                    baseExpectPos,
                    Time + MoveTime, MoveTime,
                    data.Option1, data.Option2);

                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, mirror.Conv(expectPos), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));
            }

            float lifeTime = MoveTime + 0.5f;
            if (data.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            if (note is HoldNote hold)
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    var (basePos, baseRot) = moveFunc.Invoke(t);

                    var (pos, rot) = transformConverter.Convert(
                        basePos,
                        Time, t,
                        data.Option1, data.Option2);

                    pos = mirror.Conv(pos);
                    rot = mirror.Conv(baseRot + rot);
                    note.SetPos(pos);
                    note.SetRot(rot);
                    hold.SetMaskPos(MyUtility.GetRotatedPos(new Vector2(pos.x, 0), rot));
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    var (basePos, baseRot) = moveFunc.Invoke(t);

                    var (pos, rot) = transformConverter.Convert(
                        basePos,
                        Time, t,
                        data.Option1, data.Option2);

                    pos = mirror.Conv(pos);
                    rot = mirror.Conv(baseRot + rot);
                    note.SetPos(pos);
                    note.SetRot(rot);
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
