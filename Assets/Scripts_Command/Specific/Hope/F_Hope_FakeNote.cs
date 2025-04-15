using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    //[UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Hope/フェイクノーツ"), System.Serializable]
    public class F_Hope_FakeNote : CommandBase
    {
        [SerializeField] Mirror mirror;
        [Space(20)]
        [SerializeField] RegularNoteType noteType = RegularNoteType.Hold;
        [SerializeField] float x = -3;
        [SerializeField] Lpb length = new Lpb(4);
        [Space(20)]
        [SerializeField] Vector2 toPos = new Vector2(-3, 8);
        [SerializeField] float rotate = 180;
        [SerializeField] EaseType rotEaseType = EaseType.OutBack;
        [SerializeField] Lpb actionLpb = new Lpb(1);

        protected override UniTaskVoid ExecuteAsync()
        {
            CreateNote(noteType, mirror.Conv(x), length, mirror.Conv(toPos), mirror.Conv(rotate), Delta).Forget();
            return default;
        }

        async UniTaskVoid CreateNote(RegularNoteType noteType, float x, Lpb length, Vector2 toPos, float rotate, float delta)
        {
            RegularNote note;
            if (noteType is RegularNoteType.Normal or RegularNoteType.Slide)
            {
                note = Helper.GetRegularNote(noteType);
            }
            else if (noteType is RegularNoteType.Hold)
            {
                note = Helper.GetHold(length * Speed);
            }
            else
            {
                Debug.LogError("Warning!");
                note = null;
            }

            NoteJudgeStatus judgeStatus = new NoteJudgeStatus(
                note, toPos, MoveTime + actionLpb.Time - delta, length, NoteJudgeStatus.ExpectType.Static);
            Helper.NoteInput.AddExpect(judgeStatus);

            // 普通に落下 
            delta = await WhileYieldAsync(MoveTime, t =>
            {
                note.SetPos(new Vector3(x, (MoveTime - t) * Speed));
            }, delta);

            MoveNote(note, toPos, actionLpb, delta).Forget();

            var rotEasing = new Easing(0, rotate, actionLpb.Time, rotEaseType);
            WhileYield(actionLpb.Time, t =>
            {
                note.SetRot(rotEasing.Ease(t));
            }, delta);

            if (note is HoldNote hold)
            {
                hold.SetMaskPos(new Vector2(100, 100));
                delta = await WaitSeconds(actionLpb.Time - 0.1f, delta);
                hold.SetMaskPos(toPos);
            }
        }

        async UniTaskVoid MoveNote(RegularNote note, Vector3 toPos, Lpb actionLpb, float delta)
        {
            // InQuadの終端速度は2 * (endPos - startPos).magnitude / wholeTime
            // よって easeTime = 2 * (endPos - startPos).magnitude / Speedとするとキレイに繋がる
            // (InQuadとInQuartの違いはほとんどなかった)
            Vector3 startPos = note.GetPos();
            float easeTime = 2f * (toPos - startPos).magnitude / Speed;
            delta = await WaitSeconds(actionLpb.Time - easeTime, delta);

            var posEasing = new EasingVector2(note.GetPos(), toPos, easeTime, EaseType.InQuad);
            delta = await WhileYieldAsync(easeTime, t =>
            {
                note.SetPos(posEasing.Ease(t));
            }, delta);

            float toDir = GetRad(toPos - startPos);
            var vec = new Vector3(Mathf.Cos(toDir), Mathf.Sin(toDir));
            float lifeTime = 0.3f;
            if (note is HoldNote hold)
            {
                lifeTime += hold.GetLength() / Speed;
            }
            WhileYield(lifeTime, t =>
            {
                note.SetPos(toPos + Speed * t * vec);
            }, delta);


            static float GetRad(Vector3 vec)
            {
                return Mathf.Atan2(vec.y, vec.x);
            }
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "FakeNote";
        }

        protected override string GetSummary()
        {
            return noteType + mirror.GetStatusText();
        }
#endif
    }
}