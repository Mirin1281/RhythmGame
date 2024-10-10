using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public abstract class Generator_2D : NoteGeneratorBase, IInversable
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse { get => isInverse; set => isInverse = value; }
        
        /// <summary>
        /// IsInverseがtrueの時、-1倍して返します
        /// </summary>
        protected float ConvertIfInverse(float x) => x * (isInverse ? -1 : 1);

        void IInversable.SetToggleInverse()
        {
            isInverse = !isInverse;
        }

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + 0.2f;

        protected NoteBase_2D Note(float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            Vector3 startPos = new (ConvertIfInverse(x), StartBase, -0.04f);
            DropAsync(note, startPos, delta).Forget();

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            Helper.NoteInput.AddExpect(note, expectTime);
            return note;
        }
        protected HoldNote Hold(float x, float length, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed);
            Vector3 startPos = new (ConvertIfInverse(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            DropAsync(hold, startPos, delta).Forget();

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            return hold;
        }
        protected async UniTask<float> LoopNote(float lpb, NoteType type, params float?[] nullableXs)
        {
            foreach(var nullableX in nullableXs)
            {
                if(nullableX is float x)
                {
                    Note(x, type, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }

        protected async UniTask DropAsync(NoteBase_2D note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed * Vector3.down;
            while (note.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await Helper.Yield();
            }
        }
    }
}