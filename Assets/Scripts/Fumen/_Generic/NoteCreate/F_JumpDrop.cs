using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "飛び上がる", -60), System.Serializable]
    public class F_JumpDrop : NoteCreateBase<NoteData>
    {
        [SerializeField] float defaultRadius = 10;
        [SerializeField] float defaultHeight = 10;

        [Header("オプション1 : 角度,  オプション2 : 高さ")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: -1, option2: -1) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
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

            float radius = data.Option1 == -1 ? defaultRadius : data.Option1;
            float height = data.Option2 == -1 ? defaultHeight : data.Option2;

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                if (t < MoveTime)
                {
                    float dir = (MoveTime - t) * Speed / radius;
                    note.SetPos(mirror.Conv(new Vector3(data.X * Mathf.Cos(dir), height * Mathf.Sin(dir))));
                }
                else // ロングの場合、始点を取った後は真っ直ぐ落とす
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                    note.SetRot(0);
                }
            });
        }
    }
}