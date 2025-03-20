using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "デフォルト", -100), System.Serializable]
    public class F_General : NoteCreateBase<NoteData>, IFollowableCommand
    {
        [SerializeField] TransformConverter transformConverter;
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        public (Vector3 pos, float rot) ConvertTransform(
            Vector3 basePos, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0)
        {
            return transformConverter.Convert(
                basePos,
                Time + groupTime, unGroupTime,
                option1, option2);
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            // 座標と回転の関数を定義
            (Vector3 pos, float rot) moveFunc(float t) => (new Vector3(data.X, (MoveTime - t) * Speed), 0);


            // 着弾地点を設定 //
            var baseExpectPos = moveFunc(MoveTime).pos;
            var (expectPos, _) = transformConverter.Convert(
                baseExpectPos,
                Time + MoveTime - Delta, MoveTime,
                data.Option1, data.Option2);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, mirror.Conv(expectPos), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));


            // 移動 //
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                var (basePos, baseRot) = moveFunc(t);

                // 座標変換 //
                var (pos, rot) = transformConverter.Convert(
                    basePos,
                    Time, t,
                    data.Option1, data.Option2);
                note.SetPos(mirror.Conv(pos));
                note.SetRot(mirror.Conv(baseRot + rot));
                if (note is HoldNote hold)
                {
                    hold.SetMaskPos(mirror.Conv(MyUtility.GetRotatedPos(new Vector2(pos.x, 0), rot)));
                }
            });

            /*if (note is HoldNote hold)
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    var (basePos, baseRot) = moveFunc(t);

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
                    var (basePos, baseRot) = moveFunc(t);

                    var (pos, rot) = transformConverter.Convert(
                        basePos,
                        Time, t,
                        data.Option1, data.Option2);

                    pos = mirror.Conv(pos);
                    rot = mirror.Conv(baseRot + rot);
                    note.SetPos(pos);
                    note.SetRot(rot);
                });
            }*/

            /*WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
            */
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            return;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "デフォルト";
        }

        protected override string GetSummary()
        {
            return NoteDatas?.Length + "    " + transformConverter.GetStatus() + mirror.GetStatusText();
        }
#endif
    }
}