using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

namespace NoteGenerating
{
    // 生成 → 移動
    // AddExpectを動的に設定できたら面白そう
    [AddTypeMenu("Lyrith/2 回転して着地"), System.Serializable]
    public class F_Lyrith2 : Generator_Common
    {
        [SerializeField] float startY = 7f;
        [SerializeField] NoteType noteType = NoteType.Flick;
        [SerializeField] float moveTime = 1.37f;

        protected override async UniTask GenerateAsync()
        {
            await WaitOnTiming();
            Create(new Vector2(-3f, startY), 720f);
            Create(new Vector2(3f, startY), -720f);
            await Helper.WaitSeconds(moveTime);
            //Helper.CameraMover.Shake(10, 0.4f);
        }

        void Create(Vector2 startPos, float rotationSpeed)
        {
            NoteBase_2D note = Helper.GetNote2D(noteType);
            WhileYield(moveTime, t =>
            {
                note.SetRotate(t.Ease(rotationSpeed, 0f, moveTime, EaseType.OutCubic));
                note.SetPos(startPos - new Vector2(0, t.Ease(0f, startPos.y, moveTime, EaseType.InQuint)));
            });
            Helper.NoteInput.AddExpect(note, moveTime, isCheckSimultaneous: true);
        }
    }
}