using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "2軸の揺らし", -60), System.Serializable]
    public class F_2ShakeLoop : NoteCreateBase<NoteData>
    {
        [SerializeField] float deg = 3f;
        [SerializeField] Lpb frequency = new Lpb(1f);
        [SerializeField] Lpb phase = new Lpb(2);
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [Space(10)]
        [SerializeField] bool createLine = false;
        [SerializeField] Lpb lineLifeLpb = new Lpb(4);
        [SerializeField] float lineLifeCount = 16;
        [SerializeField] float lineAlpha = 1f;

        [Header("オプション1 : 左右どちらのグループに所属するか (0 or 1)")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 0) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override async UniTaskVoid ExecuteAsync()
        {
            Line CreateLine(bool pair, float delta)
            {
                var line = Helper.GetLine();
                line.SetPos(Vector3.zero);
                line.SetAlpha(lineAlpha);
                WhileYield(lineLifeLpb.Time * lineLifeCount, t =>
                {
                    float dir = GetDir(t + MoveTime, pair);
                    line.SetRot(mirror.Conv(dir));
                }, delta);
                return line;
            }

            base.ExecuteAsync().Forget();
            if (createLine == false) return;
            float delta = await Wait(MoveLpb, Delta);
            var line1 = CreateLine(false, delta);
            var line2 = CreateLine(true, delta);
            await Wait(lineLifeLpb * lineLifeCount);
            line1.SetActive(false);
            line2.SetActive(false);
        }

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            var w = WaitDelta;

            bool pair = data.Option1 == 0;
            SetExpect(note, data, w, pair);

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                float dir = GetDir(t + w.Time, pair);
                var pos = MyUtility.GetRotatedPos(new Vector3(data.X, (MoveTime - t) * Speed), dir);
                note.SetPos(mirror.Conv(pos));
                note.SetRot(mirror.Conv(dir));
            });
        }

        float GetDir(float time, bool pair)
        {
            if (time < MoveLpb.Time && createLine) return 0;
            bool isUp = pair ^ ((int)((time + phase.Time) / frequency.Time) % 2) == 0;
            float theta = (time + phase.Time) % frequency.Time;
            return (isUp ? 1 : -1) * theta.Ease(0, deg, frequency.Time / 2f, easeType);
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, NoteJudgeStatus.ExpectType expectType = NoteJudgeStatus.ExpectType.Y_Static)
        {
            return;
        }

        void SetExpect(RegularNote note, NoteData data, in Lpb w, bool pair)
        {
            float dir = GetDir(MoveTime + w.Time, pair);
            var pos = mirror.Conv(MyUtility.GetRotatedPos(new Vector3(data.X, 0), dir));
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, pos, MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));
        }
#if UNITY_EDITOR
        protected override string GetName()
        {
            return "2-Shake";
        }
#endif
    }
}