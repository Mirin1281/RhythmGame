using NoteCreating;
using UnityEngine;

namespace NoteCreating
{
    public class PoolManager : MonoBehaviour
    {
        [field: SerializeField] public RegularNotePool RegularPool { get; private set; }
        [field: SerializeField] public HoldNotePool HoldPool { get; private set; }
        [field: SerializeField] public ArcNotePool ArcPool { get; private set; }
        [field: SerializeField] public LinePool LinePool { get; private set; }
        [field: SerializeField] public CirclePool CirclePool { get; private set; }

        public void SetMultitapSprite(RegularNote note)
        {
            RegularPool.SetMultitapSprite(note);
        }

        public void InitPools(FumenData fumen)
        {
            RegularPool.Init(fumen.NormalPoolCount, fumen.SlidePoolCount);
            HoldPool.Init(fumen.HoldPoolCount);
            ArcPool.Init(fumen.ArcPoolCount);
            LinePool.Init(fumen.LinePoolCount);
            CirclePool.Init(fumen.CirclePoolCount);
        }

#if UNITY_EDITOR
        [ContextMenu("Apply PoolCount")]
        void ApplyPoolCount()
        {
            var inGameManager = FindAnyObjectByType<InGameManager>();
            var fumenData = inGameManager.FumenData;
            fumenData.SetPoolCount(new int[5] {
                RegularPool.MaxUseCount,
                HoldPool.MaxUseCount,
                ArcPool.MaxUseCount,
                LinePool.MaxUseCount,
                CirclePool.MaxUseCount,
            });
            UnityEditor.EditorUtility.SetDirty(fumenData);
        }
#endif
    }
}