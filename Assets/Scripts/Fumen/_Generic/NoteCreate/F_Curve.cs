using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu("◆ノーツ生成 カーブノーツ", -60)]
    public class F_Curve : NoteCreateBase<NoteDataAdvanced>
    {
        [Header("オプション1 : カーブの半径 値が小さいほど効果が大きくなります")]
        [SerializeField] NoteDataAdvanced[] noteDatas = new NoteDataAdvanced[] { new(length: new Lpb(4), option1: 15) };
        protected override NoteDataAdvanced[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteDataAdvanced data)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }


            AddExpect();
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                if (t < MoveTime)
                {
                    float dir = (MoveTime - t) * Speed / data.Option1;
                    note.SetPos(mirror.Conv(new Vector3(data.X - data.Option1, 0) + data.Option1 * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir))));
                    note.SetRot(mirror.Conv(dir * Mathf.Rad2Deg));
                }
                else // ロングの場合、始点を取った後は真っ直ぐ落とす
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, StartBase - t * Speed)));
                    note.SetRot(0);
                }
            });

            if (data.Option2 != 0) Debug.LogWarning("未使用の値");
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "カーブノーツ";
        }
#endif
    }
}