using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu("◆ノーツ生成 曲がって落下", -60), System.Serializable]
    public class F_Curve : NoteCreateBase<NoteDataAdvanced>
    {
        [SerializeField] NoteDataAdvanced[] noteDatas = new NoteDataAdvanced[] { new(0, option1: 15) };
        protected override NoteDataAdvanced[] NoteDatas => noteDatas;

        protected override async UniTask MoveAsync(RegularNote note, NoteDataAdvanced data)
        {
            await UniTask.CompletedTask;
            if (data.Option2 != 0) Debug.LogWarning("未使用の値");

            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, Helper.GetTimeInterval(data.Length), expectType));
            }


            float moveTime = MoveTime;
            WhileYield(8f, t =>
            {
                if (note.IsActive == false) return;
                if (t < moveTime)
                {
                    float dir = (moveTime - t) * Speed / data.Option1;
                    note.SetPos(mirror.Conv(new Vector3(data.X - data.Option1, 0) + data.Option1 * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir))));
                    note.SetRot(mirror.Conv(dir * Mathf.Rad2Deg));
                }
                else // ロングの場合、始点を取った後は真っ直ぐ落とす
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, GetStartBase() - t * Speed)));
                    note.SetRot(0);
                }
            });
            AddExpect();
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "カーブノーツ";
        }
#endif
    }
}