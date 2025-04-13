using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◇判定の発生(拡張)"), System.Serializable]
    public class F_JudgeCreateAdvanced : CommandBase
    {
        [Serializable]
        struct JudgeData
        {
            [SerializeField] Lpb wait;
            [SerializeField] Vector2 pos;
            [SerializeField] float rotate;
            [SerializeField] float width;
            public readonly Lpb Wait => wait;
            public readonly Vector2 Pos => pos;
            public readonly float Rotate => rotate;
            public readonly float Width => width;

            public JudgeData(Lpb wait, Vector2 pos, float rotate = 0, float width = 1f)
            {
                this.wait = wait;
                this.pos = pos;
                this.rotate = rotate;
                this.width = width;
            }

            public JudgeData(bool _)
            {
                this.wait = default;
                this.pos = default;
                this.rotate = default;
                this.width = 1f;
            }
        }

        [SerializeField] Mirror mirror;
        [Space(20)]
        [SerializeField] RegularNoteType judgeNoteType = RegularNoteType.Slide;
        [SerializeField] bool isVerticalRange;
        [SerializeField] Vector2 basePos;
        [SerializeField] Lpb addDelay;
        [SerializeField] JudgeData[] judgeDatas = new JudgeData[] { new(default) };

        protected override async UniTaskVoid ExecuteAsync()
        {
            foreach (var data in judgeDatas)
            {
                await Wait(data.Wait);
                CreateJudge(data);
            }
        }

        void CreateJudge(JudgeData data)
        {
            float expectTime = MoveTime + addDelay.Time - Delta;
            Vector2 judgePos = mirror.Conv(data.Pos + basePos);
            NoteJudgeStatus judgeStatus = null;
            if (data.Width == 1)
            {
                judgeStatus = new NoteJudgeStatus(
                    judgeNoteType, judgePos, expectTime, isVerticalRange, mirror.Conv(data.Rotate));
            }
            else
            {
                var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                slide.SetPos(judgePos);
                slide.SetRot(data.Rotate);
                slide.IsVerticalRange = true;
                slide.SetWidth(data.Width);
                slide.SetRendererEnabled(false);
                judgeStatus = new NoteJudgeStatus(slide, judgePos, expectTime, expectType: NoteJudgeStatus.ExpectType.Static);
            }
            Helper.NoteInput.AddExpect(judgeStatus);
        }

#if UNITY_EDITOR

        protected override string GetSummary()
        {
            return judgeDatas.Length + mirror.GetStatusText();
        }

        protected override string GetName()
        {
            return "JudgeCreate_Ad";
        }
#endif
    }
}