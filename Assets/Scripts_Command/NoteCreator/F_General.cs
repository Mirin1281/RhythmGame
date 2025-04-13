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

        public void ConvertItem(
            ItemBase item, float groupTime, float unGroupTime, float option1 = 0, float option2 = 0)
        {
            transformConverter.Convert(
                item, mirror,
                Time + groupTime, unGroupTime,
                option1, option2);
        }

        void SetExpectAndMove(RegularNote note, NoteData data, System.Func<float, (Vector3, float)> transformFunc)
        {
            // 着弾地点を設定 //
            note.SetPosAndRot(transformFunc.Invoke(MoveTime));
            transformConverter.Convert(
                note, mirror,
                Time + MoveTime - Delta, MoveTime,
                data.Option1, data.Option2);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, note.GetPos(), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));

            // ノーツの生存時間を求める //
            float lifeTime = MoveTime + 0.2f;
            if (data.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            // 移動 //
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                var (pos, rot) = transformFunc(t);
                note.SetPosAndRot(pos, rot);
                if (note is HoldNote hold)
                {
                    hold.SetLength(data.Length * Speed);
                    hold.SetMaskPos(new Vector2(pos.x, 0));
                }

                // 座標変換 //
                transformConverter.Convert(
                    note, mirror,
                    Time, t,
                    data.Option1, data.Option2);
            });
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            // 座標と角度の関数を定義
            (Vector3, float) posAndRotFunc(float t) => (new Vector3(data.X, (MoveTime - t) * Speed), 0);

            SetExpectAndMove(note, data, posAndRotFunc);


            // 着弾地点を設定 //
            /*note.SetPosAndRot(moveFunc(MoveTime), 0);
            transformConverter.Convert(
                note, mirror,
                Time + MoveTime - Delta, MoveTime,
                data.Option1, data.Option2);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, note.GetPos(), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));*/
            /*SetExpect(note, data, posAndRotFunc);

            // 移動 //
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                Move(note, data, posAndRotFunc, t);

                var pos = moveFunc(t);
                note.SetPosAndRot(pos, 0);
                if (note is HoldNote hold)
                {
                    hold.SetLength(data.Length * Speed);
                    hold.SetMaskPos(new Vector2(pos.x, 0));
                }

                // 座標変換 //
                transformConverter.Convert(
                    note, mirror,
                    Time, t,
                    data.Option1, data.Option2);
            });*/
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