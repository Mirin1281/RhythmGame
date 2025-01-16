using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("AprilRabbit/エフェクト ノーツ落下(上昇)"), System.Serializable]
    public class F_AprilRabbit_EffectNoteDrop : Command_General
    {
        [SerializeField] RegularNoteType noteType = RegularNoteType.Flick;
        protected override async UniTask ExecuteAsync()
        {
            await WaitOnTiming();

            var rand = new System.Random(221);
            await Wait(16);
            await DropNote(new Vector2(0, 6)).Wait(16);
            await DropNote(new Vector2(2, 5.5f)).Wait(16);
            await DropNote(new Vector2(-3, 5.5f)).Wait(8);
            await DropNote(new Vector2(5, 5)).Wait(8);
            await DropNote(new Vector2(-7, 5)).Wait(8);
            await DropNote(new Vector2(-8.5f, 4.5f)).Wait(8);


            F_AprilRabbit_EffectNoteDrop DropNote(Vector2 startPos)
            {
                DropNoteAsync(startPos, noteType).Forget();
                return this;
            }

            async UniTask DropNoteAsync(Vector2 startPos, RegularNoteType noteType = RegularNoteType.Slide)
            {
                var note = Helper.GetNote(noteType);
                note.SetAlpha(0.5f);
                int a = rand.Next(0, 2) == 0 ? 1 : -1;
                float time = 0.5f;
                await WhileYieldAsync(time, t =>
                {
                    var pos = startPos + 10f * t * Vector2.up;
                    note.SetPos(new Vector3(Inv(pos.x), pos.y));
                    note.SetRot(Inv(t.Ease(90f, 450, time, EaseType.OutQuad) * a));
                    note.SetAlpha(t.Ease(0.5f, 0f, time, EaseType.OutQuad));
                });
                note.SetActive(false);
            }
        }

        protected override string GetName()
        {
            return "EffectNote";
        }
    }
}
