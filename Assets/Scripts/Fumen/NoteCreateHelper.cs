using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public Metronome Metronome => Metronome.Instance;
        public CancellationToken Token => destroyCancellationToken;

        public RegularNote GetRegularNote(RegularNoteType type, Transform parentTs = null)
        {
            var note = PoolManager.RegularPool.GetNote(type);
            if (parentTs != null)
            {
                note.transform.SetParent(parentTs);
                note.transform.localRotation = default;
            }
            return note;
        }
        public HoldNote GetHold(float length, Transform parentTs = null)
        {
            var hold = PoolManager.HoldPool.GetNote();
            hold.SetLength(length);
            if (parentTs != null)
            {
                hold.transform.SetParent(parentTs);
                hold.transform.localRotation = default;
            }
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


        public UniTask Yield(CancellationToken token = default, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (token == default)
            {
                return UniTask.Yield(timing, Token);
            }
            return UniTask.Yield(timing, token);
        }
        public async UniTask WaitSeconds(float wait, CancellationToken token = default)
        {
            if (token == default)
            {
                token = Token;
            }
            float baseTime = Metronome.CurrentTime;
            float time = 0f;
            while (time < wait)
            {
                time = Metronome.CurrentTime - baseTime;
                await UniTask.Yield(token);
            }
        }

        public float GetTimeInterval(float lpb, int num = 1)
        {
            if (lpb == 0) return 0;
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                return 240f / FumenDebugUtility.DebugBPM / lpb * num;
            }
#endif
            return 240f / Metronome.Bpm / lpb * num;
        }
    }
}
