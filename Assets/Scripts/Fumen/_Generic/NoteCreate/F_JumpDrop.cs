using Cysharp.Threading.Tasks;
using UnityEngine;

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

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            float radius = data.Option1 is -1 or 0 ? defaultRadius : data.Option1;
            float height = data.Option2 is -1 or 0 ? defaultHeight : data.Option2;

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