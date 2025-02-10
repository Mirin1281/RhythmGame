using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public class NoteCreateHelper : MonoBehaviour
    {
        [field: SerializeField] public PoolManager PoolManager { get; private set; }
        [field: SerializeField] public NoteInput NoteInput { get; private set; }
        [field: SerializeField] public CameraMover CameraMover { get; private set; }
#if UNITY_EDITOR
        [field: SerializeField] public DebugSphere DebugSpherePrefab { get; private set; }
#endif

        public CancellationToken Token => destroyCancellationToken;

        public RegularNote GetRegularNote(RegularNoteType type)
        {
            return PoolManager.RegularPool.GetNote(type);
        }
        public HoldNote GetHold(Lpb length)
        {
            var hold = PoolManager.HoldPool.GetNote();
            hold.SetLength(length);
            return hold;
        }
        public ArcNote GetArc()
        {
            return PoolManager.ArcPool.GetNote();
        }
        public Line GetLine()
        {
            return PoolManager.LinePool.GetLine();
        }
        public Circle GetCircle()
        {
            return PoolManager.CirclePool.GetCircle();
        }
    }
}
