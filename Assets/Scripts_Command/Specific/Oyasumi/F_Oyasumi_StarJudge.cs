using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Oyasumi/星エフェクト+判定"), System.Serializable]
    public class F_Oyasumi_StarJudge : CommandBase
    {
        [Space(10)]
        [SerializeField] Vector2 inputPos = new Vector2(0, 0);
        [SerializeField] Vector2 basePos = new Vector2(0, 4);
        [SerializeField] Lpb appearLpb = new Lpb(8);
        [Space(10)]
        [SerializeField] int corner = 5;
        [SerializeField] int sideCount = 2;
        [SerializeField] int size = 1;
        [SerializeField] int density = 2;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb - appearLpb);
            var points = DrawPolygonCulculator.GetPolygonPoses(
                corner: corner,
                sideCount: sideCount,
                size: size,
                density: density
            );

            var rand = Unity.Mathematics.Random.CreateFromIndex(21345);

            float delta = Delta;
            int count = 16;
            var sizeEasing = new Easing(1, 7, count, EaseType.OutQuad);
            for (int i = 0; i < count; i++)
            {
                float dir = (90 - i * (720 / (count - 1))) * Mathf.Deg2Rad;
                var pos = sizeEasing.Ease(i) * new Vector2(Mathf.Cos(dir), Mathf.Sin(dir));
                CreateJudge(pos, delta);
                var randStartRot = rand.NextFloat(0, 90f);
                var randRot = rand.NextFloat(-60, 60f);
                for (int k = 0; k < corner; k++)
                {
                    CreateStar(pos, points[k], randStartRot, randRot, delta).Forget();
                }
                delta = await Wait(new Lpb(16), delta);
            }
        }

        void CreateJudge(Vector2 pos, float delta)
        {
            float expectTime = appearLpb.Time - delta;
            Vector2 judgePos = pos + basePos;
            float rotate = Mathf.Atan2(inputPos.y - judgePos.y, inputPos.x - judgePos.x) * Mathf.Rad2Deg + 90;
            NoteJudgeStatus judgeStatus = new NoteJudgeStatus(RegularNoteType.Slide, judgePos, expectTime, true, rotate);
            Helper.NoteInput.AddExpect(judgeStatus);
        }

        async UniTaskVoid CreateStar(Vector2 basePos, Vector2 pos, float startRot, float rot, float delta)
        {
            var item = Helper.GetRegularNote(RegularNoteType.Slide);
            item.SetPos(MyUtility.GetRotatedPos(pos / 2.3f, startRot) + this.basePos + basePos);
            item.FadeIn(0.15f, 0.3f);

            float baseRot = DrawPolygonCulculator.GetAim(Vector2.zero, pos) + 90 + startRot;
            float baseTime = CurrentTime - delta;
            float t = 0;
            while (t < appearLpb.Time)
            {
                t = CurrentTime - baseTime;
                item.SetRot(baseRot + t.Ease(0, rot, 0.3f, EaseType.OutQuad));
                await Yield();
            }
            item.SetActive(false);
        }
    }
}