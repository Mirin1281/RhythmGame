using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "4方向", -60), System.Serializable]
    public class F_4Direction : NoteCreateBase<NoteData>
    {
        enum Direction { Up, Down, Left, Right }

        [Header("オプション: 0:上から, 1:下, 2:左, 3:右")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            Direction dir = data.Option1 switch
            {
                0 => Direction.Up,
                1 => Direction.Down,
                2 => Direction.Left,
                3 => Direction.Right,
                _ => throw new System.ArgumentOutOfRangeException(data.Option1.ToString())
            };
            float deg = dir switch
            {
                Direction.Up => 0,
                Direction.Down => 180,
                Direction.Left => -90,
                Direction.Right => 90,
                _ => default
            };
            Vector3 basePos = dir switch
            {
                Direction.Up => new Vector3(0, 0),
                Direction.Down => new Vector3(0, 8),
                Direction.Left => new Vector3(-8, 4),
                Direction.Right => new Vector3(8, 4),
                _ => default
            };
            int inverse = dir switch
            {
                Direction.Up => 1,
                Direction.Down => -1,
                Direction.Left => -1,
                Direction.Right => 1,
                _ => default
            };

            var vec = new Vector3(Mathf.Cos((deg + 90) * Mathf.Deg2Rad), Mathf.Sin((deg + 90) * Mathf.Deg2Rad));
            var toPos = basePos + inverse * data.X * new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
            if (HoldNote.TryParse(note, out var hold))
            {
                hold.SetRot(deg);
                hold.SetMaskPos(toPos);
            }
            else
            {
                note.SetRot(deg);
            }

            Helper.NoteInput.AddExpect(new NoteJudgeStatus(note, toPos, MoveTime - Delta, data.Length, ExpectType.Static));
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                var pos = (MoveTime - t) * Speed * vec + toPos;
                note.SetPos(mirror.Conv(pos));
            });
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, ExpectType expectType = ExpectType.Y_Static)
        {
            return;
        }
    }
}