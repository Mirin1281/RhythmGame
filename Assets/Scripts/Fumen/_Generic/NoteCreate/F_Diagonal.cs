using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "角度をつけて落下(Obsolute)", -60), System.Serializable]
    public class F_Diagonal : NoteCreateBase<NoteData>
    {
        [Header("ノーツに角度をつけない場合はP_Diagonalを使用してください")]

        [Header("オプション1 : ノーツのやって来る角度")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            note.SetRot(mirror.Conv(data.Option1));
            if (note is HoldNote hold)
            {
                hold.SetMaskRot(0); // マスクの角度はつけなくていい
            }
            float xSpeed = Mathf.Cos((data.Option1 + 90) * Mathf.Deg2Rad);
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X + xSpeed * (MoveTime - t) * Speed, (MoveTime - t) * Speed)));
            });
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Diagonal";
        }
#endif
    }
}