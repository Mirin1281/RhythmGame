using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "デフォルト", -100), System.Serializable]
    public class F_General : NoteCreateBase<NoteData>
    {
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            DropAsync(note, mirror.Conv(data.X)).Forget();

            /*WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
            */
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "デフォルト";
        }
#endif
    }
}