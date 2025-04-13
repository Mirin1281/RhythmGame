using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/サビ前ダミー譜面"), System.Serializable]
    public class F_Quiela_Dummy : CommandBase
    {
        [SerializeField] Lpb interval = new Lpb(2);
        [SerializeField] float speedRate = 0.3f;
        [SerializeField] float deltaY = 8;
        [SerializeField] NoteData[] noteDatas1;
        [SerializeField] NoteData[] noteDatas2;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb);

            CreateNotes(noteDatas1, delta, 1);
            delta = await Wait(interval, delta);
            CreateNotes(noteDatas2, delta, 2);
            delta = await Wait(interval, delta);
            delta = await Wait(new Lpb(8), delta);
            SlideEffect(delta).Forget();
        }

        void CreateNotes(NoteData[] noteDatas, float delta, int index)
        {
            float y = interval.Time * Speed;
            for (int i = 0; i < noteDatas.Length; i++)
            {
                var d = noteDatas[i];
                y += d.Wait.Time * Speed;
                if (d.Type == RegularNoteType._None) continue;
                var note = Helper.GetRegularNote(d);
                if (note is HoldNote hold)
                {
                    hold.SetMaskPos(new Vector2(100, 100));
                    hold.SetLength(d.Length * Speed);
                }
                MoveNote(note, new Vector2(d.X, y), delta, index).Forget();
            }
        }

        async UniTaskVoid MoveNote(RegularNote note, Vector2 startPos, float delta, int index)
        {
            delta = await WhileYieldAsync(interval.Time, t =>
            {
                note.SetPos(new Vector3(startPos.x, deltaY - (startPos.y - t * Speed)));
            }, delta);

            if (index == 1)
            {
                note.SetActive(false);
            }
            else if (index == 2)
            {
                var tmpY = note.GetPos().y / 2f;
                await WhileYieldAsync(interval.Time * 2f, t =>
                {
                    note.SetPos(new Vector3(startPos.x, (tmpY - t + (new Lpb(2f) - new Lpb(4)).Time) * Speed));
                }, delta);
                note.SetActive(false);
            }
        }

        async UniTaskVoid SlideEffect(float delta)
        {
            var rand = new System.Random(225);
            for (int i = 0; i < 6; i++)
            {
                var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                Move(slide, rand, delta).Forget();
                delta = await Wait(new Lpb(16), delta);
            }


            async UniTaskVoid Move(RegularNote slide, System.Random rand, float delta)
            {
                var easing = new Easing(-45, 225, new Lpb(2f).Time, EaseType.OutQuad);
                float diff = rand.GetInt(0, 7);
                await WhileYieldAsync(new Lpb(2f).Time, t =>
                {
                    float dir = easing.Ease(t);
                    slide.SetPos(MyUtility.GetRotatedPos(new Vector3(10 - diff, 0), dir, Vector2.zero));
                    slide.SetRot(dir + 90);
                }, delta);
                await Wait(new Lpb(4f));
                slide.SetActive(false);
            }
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Q_Dummy";
        }
#endif
    }
}