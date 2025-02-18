using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [MovedFrom(false, null, null, "F_NonJudge")]
    [AddTypeMenu(FumenPathContainer.NoteCreate + "判定エフェクトなし", -60), System.Serializable]
    public class F_InvisibleJudgeEffect : NoteCreateBase<NoteData>
    {
        [Header("注意: ロングの終点は消していません")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }

            AddExpect(new Vector2(data.X, -10), ExpectType.Static);
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            note.IsVerticalRange = true;

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "InvisibleJudge";
        }
#endif
    }
}