using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "判定しない", -60), System.Serializable]
    public class F_NonJudge : NoteCreateBase<NoteData>
    {
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4)) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            WhileYield(lifeTime, t => // 普通に落下
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
            UniTask.Void(async () =>
            {
                float time = MoveTime - Delta;
                if (note is HoldNote hold)
                {
                    time += data.Length.Time;
                }
                await MyUtility.WaitSeconds(time, Helper.Token);
                note.SetActive(false);
            });
        }
    }
}