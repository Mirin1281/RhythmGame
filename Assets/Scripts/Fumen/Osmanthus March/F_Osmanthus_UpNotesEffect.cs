using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("Osmanthus/上昇するノーツエフェクト"), System.Serializable]
    public class F_Osmanthus_UpNotesEffect : CommandBase
    {
        [SerializeField] int seed = 222;
        [SerializeField] int count = 70;

        protected override async UniTask ExecuteAsync()
        {
            await WaitOnTiming();

            var rand = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                var flick = Helper.GetNote(RegularNoteType.Flick);
                Helper.PoolManager.SetSimultaneousSprite(flick);
                flick.SetRot(90);
                flick.SetAlpha(0.2f);
                flick.transform.localScale = rand.GetFloat(0.5f, 1.5f) * Vector3.one;
                MoveAsync(flick, rand.GetInt(-12, 12), rand.GetFloat(0.5f, 4f)).Forget();
                await Wait(rand.GetFloat(3f, 6f));
            }
        }

        async UniTask MoveAsync(RegularNote note, float x, float time)
        {
            var easing = new Easing(-6f, 14f, time, EaseType.OutQuad);
            await WhileYieldAsync(time, t =>
            {
                note.SetPos(new Vector3(x, easing.Ease(t)));
            });
            note.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "UpNotesEffect";
        }
#endif
    }
}
