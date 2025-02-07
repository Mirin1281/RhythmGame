using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu("◆一般 ノーツ生成", -100), System.Serializable]
    public class F_General : NoteCreateBase<NoteData>
    {
        [SerializeField] NoteData[] noteDatas;
        protected override NoteData[] NoteDatas => noteDatas;

        protected override async UniTask MoveAsync(RegularNote note, NoteData data)
        {
            await UniTask.CompletedTask;

            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, Helper.GetTimeInterval(data.Length), expectType));
            }

            /*AddExpect();
            float baseTime = CurrentTime - Delta;
            float time = 0f;
            while (note.IsActive && time < 8f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(new Vector3(data.X, GetStartBase() - time * Speed));
                await Helper.Yield();
            }*/
            WhileYield(8f, t =>
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
            AddExpect();
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "汎用2D";
        }
#endif
    }
}