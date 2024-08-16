using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace NoteGenerating
{
    // 生成 → 移動
    // AddExpectを動的に設定できたら面白そう
    [AddTypeMenu("Lyrith/2 回転して着地"), System.Serializable]
    public class F_Lyrith2 : Generator_Type1
    {
        [SerializeField] float moveTime = 1.37f;
        protected override async UniTask GenerateAsync()
        {
            Create(new Vector2(-2f, 2f), 720f);
            Create(new Vector2(2f, 2f), -720f);
            var camera = Camera.main;
            await UniTask.Delay(System.TimeSpan.FromSeconds(moveTime), cancellationToken: Helper.Token);
            _ = camera.DOShakePosition(0.3f, 1, 20);
            _ = camera.DOShakeRotation(0.3f, 1, 20);
        }

        void Create(Vector2 startPos, float rotationSpeed)
        {
            var note = Helper.NormalNotePool.GetNote();
            WhileYield(moveTime, t => 
            {
                note.SetRotate(t.Ease(rotationSpeed, 0f, moveTime, EaseType.OutCubic));
                note.SetPos(startPos - new Vector2(0, t.Ease(0f, startPos.y - From, moveTime, EaseType.InQuint)));
            });
            float expectTime = CurrentTime + moveTime;
            Helper.NoteInput.AddExpect(new NoteExpect(note, new Vector2(startPos.x, From), expectTime));
        }
    }
}