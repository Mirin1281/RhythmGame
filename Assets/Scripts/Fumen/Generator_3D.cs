using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteGenerating
{
    public abstract class Generator_3D : NoteGeneratorBase, IInversable
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

        protected virtual float Speed => RhythmGameManager.Speed3D;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + 0.2f;

        /*protected async UniTask<float> SkyLoop(float lpb, params Vector2?[] nullablePoses)
        {
            foreach(var nullablePos in nullablePoses)
            {
                if(nullablePos is Vector2 pos)
                {
                    Sky(pos, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }*/

        protected SkyNote Sky(Vector2 pos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            SkyNote sky = Helper.GetSky();
            var startPos = new Vector3(ConvertIfInverse(pos.x), pos.y, StartBase);
            DropAsync(sky, startPos, delta).Forget();

            float distance = startPos.z - Speed * delta;
            float expectTime = distance / Speed + CurrentTime;
            Helper.NoteInput.AddExpect(sky, startPos, expectTime);
            return sky;
        }

        protected ArcNote Arc(ArcCreateData[] datas, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, IsInverse).Forget();
            var startPos = new Vector3(0, 0f, StartBase);
            DropAsync(arc, startPos, delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed * Vector3.back;
            while (note.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await Helper.Yield();
            }
        }
    }
}