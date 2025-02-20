using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/スライド円ノーツ"), System.Serializable]
    public class F_Quiela_CircleNote : CommandBase
    {
        [System.Serializable]
        struct NoteData
        {
            [SerializeField] Lpb wait;
            [SerializeField] Vector2 pos;
            public readonly Lpb Wait => wait;
            public readonly Vector2 Pos => pos;
        }

        //[SerializeField] Mirror mirror;
        [SerializeField] float size = 3f;
        [SerializeField] Lpb moveLpb = new Lpb(2);
        [SerializeField] Vector2 basePos = new Vector2(0, 4);
        [SerializeField] NoteData[] noteDatas;
        EaseType easeType = EaseType.InBack;

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb - moveLpb);

            for (int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                delta = await Wait(data.Wait, delta);
                Note(data.Pos + basePos, i % 2 == 0, delta);
            }
        }

        void Note(Vector2 pos, bool invertRotate = false, float delta = 0)
        {
            int count = 3;
            float rand = UnityEngine.Random.Range(0, 180);
            for (int i = 0; i < count; i++)
            {
                var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                MoveSlide(slide, pos, i * 360f / count + rand, invertRotate).Forget();
            }

            var circle = Helper.GetCircle();
            MoveCircle(circle, pos).Forget();

            Helper.NoteInput.AddExpect(new NoteJudgeStatus(RegularNoteType.Slide, pos, moveLpb.Time - delta));
        }

        async UniTaskVoid MoveSlide(RegularNote slide, Vector2 pos, float dir, bool invertRotate = false)
        {
            var posEasing = new Easing(size * moveLpb.Time, 0, moveLpb.Time, easeType);
            var rotEasing = new Easing(dir + (invertRotate ? -1 : 1) * 180, dir, moveLpb.Time, easeType);
            await WhileYieldAsync(moveLpb.Time, t =>
            {
                slide.SetPos(pos + MyUtility.GetRotatedPos(new Vector2(0, posEasing.Ease(t)), rotEasing.Ease(t), Vector2.zero));
                slide.SetRot(rotEasing.Ease(t));
            });
            slide.SetActive(false);
        }

        async UniTaskVoid MoveCircle(Circle circle, Vector2 pos)
        {
            circle.SetPos(pos);
            circle.SetScale(size * 0.4f);
            circle.SetAlpha(0.2f);
            circle.SetWidth(0.15f);
            circle.SetScaleAsync(-0.1f, moveLpb.Time, easeType).Forget();
            circle.FadeAlphaAsync(0.7f, moveLpb.Time, easeType).Forget();
            await Wait(moveLpb);
            circle.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Q_Circle";
        }
#endif
    }
}