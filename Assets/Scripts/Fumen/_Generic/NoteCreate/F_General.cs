using Cysharp.Threading.Tasks;
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

        public (Vector3 pos, float rot) ConvertTransform(Vector3 basePos, float option = 0, float time = 0)
        {
            return transformConverter.ConvertTransform(basePos, option, Time);
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            CreateDropNote(note, data, transformConverter);

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