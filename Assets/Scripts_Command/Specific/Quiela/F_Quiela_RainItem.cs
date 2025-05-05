using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/雨のように降る"), System.Serializable]
    public class F_Quiela_RainItem : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] int count = 128;
        [SerializeField] Vector2 waitRange = new Vector2(16, 32);
        [SerializeField] RegularNoteType noteType = RegularNoteType.Slide;
        [SerializeField] int rot = 70;
        [SerializeField] float dropSpeed = 70f;
        [SerializeField] float lifeTime = 0.3f;
        [SerializeField] float alpha = 0.5f;
        [SerializeField] int seed = 4739847;

        protected override async UniTaskVoid ExecuteAsync()
        {
            var delta = await Wait(MoveLpb);
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)seed);

            for (int i = 0; i < count; i++)
            {
                var item = Helper.GetRegularNote(noteType);
                DropItem(item, rand, delta).Forget();
                var wait = new Lpb(rand.NextFloat(waitRange.x, waitRange.y));
                delta = await Wait(wait, delta);
            }
        }

        async UniTaskVoid DropItem(ItemBase item, Unity.Mathematics.Random rand, float delta)
        {
            var x = rand.NextFloat(-13, 13);
            var startPos = new Vector2(x + Mathf.Cos(rot * Mathf.Deg2Rad), 15);
            item.SetPos(startPos);
            item.SetRot(rot);
            item.SetAlpha(alpha);

            float baseTime = CurrentTime - delta;
            while (item.IsActive)
            {
                float t = CurrentTime - baseTime;
                item.SetPos(startPos - t * dropSpeed * new Vector2(Mathf.Cos(rot * Mathf.Deg2Rad), 1));
                if (t >= lifeTime) break;
                await Yield();
            }
            item.SetActive(false);
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "RainItem";
        }
#endif
    }
}
