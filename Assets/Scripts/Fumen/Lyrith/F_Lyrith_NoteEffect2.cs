using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Lyrith_BlinkNotes")]
    [AddTypeMenu("Lyrith/【演出】ノーツ上昇"), System.Serializable]
    public class F_Lyrith_NoteEffect2 : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                var slide = Helper.GetRegularNote(RegularNoteType.Slide);
                float a = i - count / 2f + 0.5f;
                slide.SetPos(new Vector2(a, a * 0.5f + 4f));
                MoveAndRotateAsync(slide).Forget();
            }
        }

        async UniTask MoveAndRotateAsync(RegularNote note)
        {
            Vector2 startPos = note.GetPos();
            float time = 0.6f;
            var vec = Vector2.up;
            await WhileYieldAsync(time, t =>
            {
                note.SetPos(startPos + vec * (t + 0.1f).Ease(0f, 13f, time, EaseType.InQuart));
                note.SetRot(90 + t * 100f);
            });
            note.SetActive(false);
        }
    }
}
