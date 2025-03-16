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

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            note.IsVerticalRange = true;

            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            base.AddExpect(note, pos - new Vector2(0, 10), length, ExpectType.Static);
        }

#if UNITY_EDITOR
        protected override string GetName()
        {
            return "InvisibleEffect";
        }
#endif
    }
}