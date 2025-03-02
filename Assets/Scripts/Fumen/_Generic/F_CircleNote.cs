using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆円ノーツ"), System.Serializable]
    public class F_CircleNote : CommandBase
    {
        [System.Serializable]
        struct NoteData
        {
            [SerializeField] Lpb wait;
            [SerializeField] RegularNoteType noteType;
            [SerializeField] Vector2 pos;
            [SerializeField] Vector2 dir;

            public readonly Lpb Wait => wait;
            public readonly RegularNoteType NoteType => noteType;
            public readonly Vector2 Pos => pos;
            public readonly Vector2 Dir => dir;

            public NoteData(Lpb wait = default, RegularNoteType noteType = RegularNoteType.Normal, Vector2 pos = default, Vector2 dir = default)
            {
                this.wait = wait;
                this.noteType = noteType;
                this.pos = pos;
                this.dir = dir;
            }
        }

        [SerializeField] Mirror mirror;
        [SerializeField] float circleSize = 1f;
        [SerializeField] Lpb moveLpb = new Lpb(2);
        [SerializeField] Vector2 basePos = new Vector2(0, 4);
        [SerializeField] EaseType easeType = EaseType.InBack;
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(noteType: RegularNoteType.Normal, dir: new Vector2(0, 90)) };

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb - moveLpb);

            for (int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                delta = await Wait(data.Wait, delta);
                Note(data, delta);
            }
        }

        void Note(in NoteData data, float delta)
        {
            var note = Helper.GetRegularNote(data.NoteType);
            var pos = mirror.Conv(data.Pos + basePos);
            MoveNote(note, pos, data.Dir).Forget();

            var circle = Helper.GetCircle();
            MoveCircle(circle, pos).Forget();

            Helper.NoteInput.AddExpect(new NoteJudgeStatus(data.NoteType, pos, moveLpb.Time - delta));
        }

        async UniTaskVoid MoveNote(RegularNote note, Vector2 pos, Vector2 dir)
        {
            note.SetPos(pos);
            var rotEasing = new Easing(dir.x, dir.y, moveLpb.Time, EaseType.OutCubic);
            await WhileYieldAsync(moveLpb.Time, t =>
            {
                note.SetRot(mirror.Conv(rotEasing.Ease(t)));
            });
            note.SetActive(false);
        }

        async UniTaskVoid MoveCircle(Circle circle, Vector2 pos)
        {
            circle.SetPos(pos);
            circle.SetScale(circleSize);
            circle.SetAlpha(0.2f);
            circle.SetWidth(0.15f);
            circle.SetScaleAsync(-0.1f, moveLpb.Time, easeType).Forget();
            circle.FadeAlphaAsync(0.9f, moveLpb.Time, easeType).Forget();
            await Wait(moveLpb);
            circle.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Circle";
        }
#endif
    }
}