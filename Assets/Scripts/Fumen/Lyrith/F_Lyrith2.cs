using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

namespace NoteGenerating
{
    // 生成 → 移動
    // AddExpectを動的に設定できたら面白そう
    [AddTypeMenu("Lyrith/2 回転して着地"), System.Serializable]
    public class F_Lyrith2 : Generator_Type1
    {
        [SerializeField] float startY = 7f;
        [SerializeField] NoteType noteType = NoteType.Flick;
        [SerializeField] float moveTime = 1.37f;

        protected override async UniTask GenerateAsync()
        {
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            Create(new Vector2(-3f, startY), 720f);
            Create(new Vector2(3f, startY), -720f);
            var camera = Camera.main;
            await WaitSeconds(moveTime);
            CameraShake(camera, 10, 0.4f);
        }

        void Create(Vector2 startPos, float rotationSpeed)
        {
            NoteBase note = Helper.PoolManager.GetNote(noteType);
            WhileYield(moveTime, t => 
            {
                note.SetRotate(t.Ease(rotationSpeed, 0f, moveTime, EaseType.OutCubic));
                note.SetPos(startPos - new Vector2(0, t.Ease(0f, startPos.y, moveTime, EaseType.InQuint)));
            });
            float expectTime = CurrentTime + moveTime;
            Helper.NoteInput.AddExpect(new NoteExpect(note, new Vector2(startPos.x, 0), expectTime));
        }

        void CameraShake(Camera camera, float strength, float time)
        {
            camera.transform.localRotation = Quaternion.Euler(0f, 0f, strength);
            WhileYield(time, t => 
            {
                camera.transform.localRotation = Quaternion.Euler(0f, 0f, t.Ease(strength, 0, time, EaseType.OutBack));
            });
        }
    }
}