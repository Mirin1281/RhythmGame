using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆連続的なノーツ", -80), System.Serializable]
    public class F_ContinuousNotes : CommandBase
    {
        [SerializeField] Mirror mirror;
        [Space(10)]
        [SerializeField] int count = 16;
        [SerializeField] int trimStart = 0;
        [SerializeField, Tooltip("NormalかSlideを指定してください")] RegularNoteType noteType = RegularNoteType.Slide;
        [SerializeField] Lpb wait = new Lpb(16);
        [Space(10)]
        [SerializeField] Vector2 easeX = new Vector2(-5f, 5f);
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] float easeTime = 16;
        [Space(10)]
        [SerializeField, CommandSelect] CommandData commandData;
        [SerializeField] float option;

        protected override async UniTaskVoid ExecuteAsync()
        {
            IFollowableCommand followable = null;
            if (commandData != null)
            {
                followable = commandData.GetCommand() as IFollowableCommand;
#if UNITY_EDITOR
                if (followable == null)
                {
                    Debug.LogWarning($"{commandData.GetName()} を{nameof(IFollowableCommand)}に変換できませんでした");
                }
#endif
            }

            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for (int i = 0; i < count; i++)
            {
                if (i >= trimStart)
                {
                    float x = easing.Ease(i * ((float)(count + 1) / count));
                    CreateDropNote(mirror.Conv(x), noteType, default, followable, option);
                }
                await Wait(wait);
            }
        }

        void CreateDropNote(float x, RegularNoteType type, Lpb holdLength, IFollowableCommand followable = null, float option = 0)
        {
            RegularNote note = Helper.GetRegularNote(type);

            if (followable == null)
            {
                Helper.NoteInput.AddExpect(note, MoveTime - Delta, holdLength);
            }
            else
            {
                var pos = followable.ConvertTransform(new Vector3(x, 0), option, time: MoveTime).pos;
                var judgeStatus = new NoteJudgeStatus(note, pos, MoveTime - Delta, holdLength, NoteJudgeStatus.ExpectType.Static);
                Helper.NoteInput.AddExpect(judgeStatus);
            }

            float lifeTime = MoveTime + 0.5f;
            if (type == RegularNoteType.Hold)
            {
                lifeTime += holdLength.Time;
            }

            if (HoldNote.TryParse(note, out var hold))
            {
                WhileYield(lifeTime, t =>
                {
                    if (hold.IsActive == false) return;
                    var basePos = new Vector3(x, (MoveTime - t) * Speed);
                    if (followable == null)
                    {
                        hold.SetPos(basePos);
                    }
                    else
                    {
                        var (pos, rot) = followable.ConvertTransform(basePos, option);
                        hold.SetPos(pos);
                        hold.SetRot(rot);
                        hold.SetMaskPos(MyUtility.GetRotatedPos(new Vector2(pos.x, 0), rot));
                    }
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    var basePos = new Vector3(x, (MoveTime - t) * Speed);
                    if (followable == null)
                    {
                        note.SetPos(basePos);
                    }
                    else
                    {
                        var (pos, rot) = followable.ConvertTransform(basePos, option);
                        note.SetPos(pos);
                        note.SetRot(rot);
                    }
                });
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.GetNoteCommandColor();
        }

        protected override string GetSummary()
        {
            return mirror.GetStatusText();
        }

        protected override string GetName()
        {
            return "Continuous";
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
            if (noteType == RegularNoteType._None) return;
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            if (beforeClear)
                previewer.CreateGuideLine();

            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            float y = delay.Time * Speed;
            for (int i = 0; i < count; i++)
            {
                if (i >= trimStart)
                {
                    var note = Helper.GetRegularNote(noteType);
                    note.transform.SetParent(previewer.transform);
                    float x = easing.Ease(i * ((float)(count + 1) / count));
                    note.SetPos(mirror.Conv(new Vector3(x, y)));
                }

                y += wait.Time * Speed;
            }
        }
#endif
    }
}