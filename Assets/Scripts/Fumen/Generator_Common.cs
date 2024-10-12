using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public abstract class Generator_Common : NoteGeneratorBase, IInversableCommand
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse { get => isInverse; set => isInverse = value; }
        
        /// <summary>
        /// IsInverseがtrueの時、-1倍して返します
        /// </summary>
        protected float ConvertIfInverse(float x) => x * (isInverse ? -1 : 1);

        void IInversableCommand.SetToggleInverse()
        {
            isInverse = !isInverse;
        }

        protected virtual float Speed => RhythmGameManager.Speed;
        protected virtual float Speed3D => RhythmGameManager.Speed3D;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 2f * Speed + 0.2f;
        protected float StartBase3D => 2f * Speed3D + 0.2f;

        protected NoteBase_2D Note2D(float x, NoteType type, float delta = -1, bool isSpeedChangable = false, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            if(parentTs != null)
            {
                note.transform.SetParent(parentTs);
                note.transform.localRotation = default;
            }
            Vector3 startPos = new (ConvertIfInverse(x), StartBase, -0.04f);
            if(isSpeedChangable)
            {
                DropAsync_SpeedChangable(note, delta).Forget();
            }
            else
            {
                DropAsync(note, startPos, delta).Forget();
            }

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(note, expectTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
            }
            return note;


            async UniTask DropAsync_SpeedChangable(NoteBase_2D note, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    var vec = Speed * Vector3.down;
                    note.SetPos(new Vector3(ConvertIfInverse(x), StartBase, -0.04f) + time * vec);
                    await Helper.Yield();
                }
            }
        }
        protected HoldNote Hold(float x, float length, float delta = -1, bool isSpeedChangable = false, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed);
            if(parentTs != null)
            {
                hold.transform.SetParent(parentTs);
                hold.transform.localRotation = default;
            }
            Vector3 startPos = new (ConvertIfInverse(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            if(isSpeedChangable)
            {
                DropAsync_SpeedChangable(hold, delta).Forget();
            }
            else
            {
                DropAsync(hold, startPos, delta).Forget();
            }

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, holdTime, mode: NoteExpect.ExpectMode.Y_Static);
            }
            return hold;


            async UniTask DropAsync_SpeedChangable(HoldNote hold, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                while (hold.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    var vec = Speed * Vector3.down;
                    hold.SetLength(holdTime * Speed);
                    hold.SetPos(new Vector3(ConvertIfInverse(x), StartBase, -0.04f) + time * vec);
                    await Helper.Yield();
                }
            }
        }

        protected SkyNote Sky(Vector2 pos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            SkyNote sky = Helper.GetSky();
            var startPos = new Vector3(ConvertIfInverse(pos.x), pos.y, StartBase3D);
            DropAsync_3D(sky, startPos, delta).Forget();

            float distance = startPos.z - Speed3D * delta;
            float expectTime = distance / Speed3D + CurrentTime;
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
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed3D, IsInverse).Forget();
            var startPos = new Vector3(0, 0f, StartBase3D);
            DropAsync_3D(arc, startPos, delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected async UniTask<float> LoopNote(float lpb, NoteType type, params float?[] nullableXs)
        {
            foreach(var nullableX in nullableXs)
            {
                if(nullableX is float x)
                {
                    Note2D(x, type, Delta);
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
        protected async UniTask DropAsync_3D(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed3D * Vector3.back;
            while (note.IsActive && time < 5f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await Helper.Yield();
            }
        }

        /// <summary>
        /// Parentを生成します
        /// </summary>
        protected Transform CreateParent(IParentGeneratable parentGeneratable)
        {
            return parentGeneratable?.GenerateParent(Delta, Helper, IsInverse);
        }
    }
}