using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Lyrith_BlinkNotes")]
    [AddTypeMenu("Lyrith/【演出】ノーツ上昇"), System.Serializable]
    public class F_Lyrith_NoteEffect2 : Generator_Type1
    {
        protected override async UniTask GenerateAsync()
        {
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            int count = 12;
            for(int i = 0; i < count; i++)
            {
                var slide = Helper.SlideNotePool.GetNote();
                float a = i - count / 2f + 0.5f;
                slide.SetPos(new Vector2(a, a * 0.5f + 4f));
                MoveAndRotateAsync(slide).Forget();
            }
        }

        async UniTask MoveAndRotateAsync(NoteBase note)
        {
            Vector2 startPos = note.GetPos();
            float time = 0.6f;
            var vec = Vector2.up;
            await WhileYieldAsync(time, t => 
            {
                note.SetPos(startPos + vec * (t + 0.1f).Ease(0f, 13f, time, EaseType.InQuart));
                note.SetRotate(90 + t * 100f);
            });
            note.SetActive(false);
        }
    }
}
