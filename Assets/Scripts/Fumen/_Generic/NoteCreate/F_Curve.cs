using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "カーブ", -60), System.Serializable]
    public class F_Curve : NoteCreateBase<NoteData>
    {
        [SerializeField] float defaultCurve = 30;
        [SerializeField] bool isGroup;
        [SerializeField] float dirSpeed = 40f;

        [Header("オプション1 : カーブの半径 値が小さいほど効果が大きくなります")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 30) };
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

            float curve = data.Option1 is -1 or 0 ? defaultCurve : data.Option1;
            if (isGroup)
            {
                float rotateSpeed = Mathf.Abs(curve * dirSpeed * Mathf.Deg2Rad);
                Vector3 centerPos = new Vector2(-curve, 0);
                var dirEasing = new Easing(MoveTime * dirSpeed, 0, MoveTime, EaseType.OutQuad);
                Lpb w = WaitDelta;
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    if (t + w.Time < MoveTime)
                    {
                        float dir = (MoveTime - (t + w.Time)) * dirSpeed;
                        note.SetPos(mirror.Conv(MyUtility.GetRotatedPos(
                            new Vector3(data.X, (MoveTime - t) * Speed),
                            dirEasing.Ease(t + w.Time),
                            centerPos)));
                        note.SetRot(mirror.Conv(dirEasing.Ease(t + w.Time)));
                    }
                    else // ロングの場合、始点を取った後は真っ直ぐ落とす
                    {
                        note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                        note.SetRot(0);
                    }
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    if (t < MoveTime)
                    {
                        float dir = (MoveTime - t) * Speed / curve;
                        note.SetPos(mirror.Conv(new Vector3(data.X - curve, 0) + curve * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir))));
                        note.SetRot(mirror.Conv(t.Ease(MoveTime * Speed / curve, 0, MoveTime, EaseType.OutQuad) * Mathf.Rad2Deg));
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
}