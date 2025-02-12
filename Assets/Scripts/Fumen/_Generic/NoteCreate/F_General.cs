using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu("◆一般 ノーツ生成", -100), System.Serializable]
    public class F_General : NoteCreateBase<NoteData>
    {
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, Vector2.zero, MoveTime - Delta, data.Length, ExpectType.Y_Static));
            DropAsync(note, mirror.Conv(data.X)).Forget();
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "汎用2D";
        }
#endif
    }
}