using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

namespace NoteCreating
{
    // 生成 → 移動
    // AddExpectを動的に設定できたら面白そう
    [AddTypeMenu("Lyrith/2 回転して着地")]
    public class F_Lyrith2 : CommandBase
    {
        [SerializeField] float startY = 7f;
        [SerializeField] RegularNoteType noteType = RegularNoteType.Slide;
        [SerializeField] float moveTime = 1.37f;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await WaitOnTiming();
            Create(new Vector2(-3f, startY), 720f);
            Create(new Vector2(3f, startY), -720f);
            await WaitSeconds(moveTime);
            //Helper.CameraMover.Shake(10, 0.4f);
        }

        void Create(Vector2 startPos, float rotationSpeed)
        {
            RegularNote note = Helper.GetRegularNote(noteType);
            WhileYield(moveTime, t =>
            {
                note.SetRot(t.Ease(rotationSpeed, 0f, moveTime, EaseType.OutCubic));
                note.SetPos(startPos - new Vector2(0, t.Ease(0f, startPos.y, moveTime, EaseType.InQuint)));
            });
            Helper.NoteInput.AddExpect(note, moveTime, isCheckSimultaneous: true);
        }
    }
}