using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [MovedFrom(false, null, null, "F_InvisibleJudgeEffect")]
    [AddTypeMenu(FumenPathContainer.NoteCreate + "判定3種盛り", -60), System.Serializable]
    public class F_JudgeVariety : NoteCreateBase<NoteData>
    {
        enum ActionType
        {
            InvisibleJudge, // 注意: ロングの終点は消していません
            NonJudge, // 判定する瞬間に消滅します
            TouchHold, // ホールドが実質タッチ判定になり、すぐに消えます
        }

        [SerializeField] ActionType actionType = ActionType.InvisibleJudge;
        [SerializeField] TransformConverter transformConverter;

        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            if (actionType == ActionType.InvisibleJudge)
                note.IsVerticalRange = true;

            // 座標と回転の関数を定義
            (Vector3 pos, float rot) moveFunc(float t) => (new Vector3(data.X, (MoveTime - t) * Speed), 0);

            if (actionType != ActionType.NonJudge)
            {
                // 着弾地点を設定 //
                var baseExpectPos = moveFunc(MoveTime).pos + (actionType == ActionType.InvisibleJudge ? -new Vector3(0, 10) : Vector3.zero);
                note.SetPos(mirror.Conv(baseExpectPos));
                transformConverter.Convert(
                    note, mirror,
                    Time + MoveTime - Delta, MoveTime,
                    data.Option1, data.Option2);
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, note.GetPos(), MoveTime - Delta, actionType == ActionType.TouchHold ? Lpb.Zero : data.Length, NoteJudgeStatus.ExpectType.Static));
            }


            // 移動 //
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                var (basePos, baseRot) = moveFunc(t);
                note.SetPos(basePos);

                // 座標変換 //
                transformConverter.Convert(
                    note, mirror,
                    Time, t,
                    data.Option1, data.Option2);
            });

            if (actionType == ActionType.NonJudge)
            {
                UniTask.Void(async () =>
                {
                    float time = MoveTime - Delta;
                    if (note is HoldNote hold)
                    {
                        time += data.Length.Time;
                    }
                    await MyUtility.WaitSeconds(time, Helper.Token);
                    note.SetActive(false);
                });
            }
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            return;
        }

#if UNITY_EDITOR
        protected override string GetName()
        {
            return actionType.ToString();
        }
#endif
    }
}