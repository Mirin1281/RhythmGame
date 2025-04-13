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
        [field: SerializeField] public AudioWaveMeter WaveMeter { get; private set; }
#if UNITY_EDITOR
        [field: SerializeField] public DebugSphere DebugCirclePrefab { get; private set; }
#endif

        public CancellationToken Token => destroyCancellationToken;

        public RegularNote GetRegularNote(RegularNoteType type)
        {
            return PoolManager.RegularPool.GetNote(type);
        }
        public RegularNote GetRegularNote(INoteData noteData)
        {
            if (noteData.Type == RegularNoteType.Hold)
            {
                return PoolManager.HoldPool.GetNote(noteData.Length * RhythmGameManager.Speed);
            }
            return PoolManager.RegularPool.GetNote(noteData.Type);
        }

        /// <summary>
        /// Speedは考慮されていないこと注意
        /// </summary>
        public HoldNote GetHold(Lpb length)
        {
            return PoolManager.HoldPool.GetNote(length);
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
