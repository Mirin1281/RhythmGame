using NoteGenerating;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [field: SerializeField] public NormalNotePool NormalPool { get; private set; }
    [field: SerializeField] public CircleNotePool CirclePool { get; private set; }
    [field: SerializeField] public SlideNotePool SlidePool { get; private set; }
    [field: SerializeField] public FlickNotePool FlickPool { get; private set; }
    [field: SerializeField] public HoldNotePool HoldPool { get; private set; }
    [field: SerializeField] public SkyNotePool SkyPool { get; private set; }
    [field: SerializeField] public ArcNotePool ArcPool { get; private set; }
    [field: SerializeField] public LinePool LinePool { get; private set; }

    public NoteBase_2D GetNote2D(NoteType type, int index = 0) => type switch
    {
        NoteType.Normal => NormalPool.GetNote(index),
        NoteType.Slide => SlidePool.GetNote(),
        NoteType.Flick => FlickPool.GetNote(),
        _ => throw new System.Exception()
    };

    public void SetSimultaneousSprite(NoteBase_2D note)
    {
        if(note == null) return;
        var sprite = GetSimultaneousSprite(note.Type);
        if(sprite == null) return;
        note.SetSprite(sprite);


        Sprite GetSimultaneousSprite(NoteType type) => type switch
        {
            NoteType.Normal => NormalPool.GetSimultaneousSprite(),
            NoteType.Slide => SlidePool.GetSimultaneousSprite(),
            NoteType.Flick => FlickPool.GetSimultaneousSprite(),
            NoteType.Hold => null,
            NoteType.Circle => null,
            /*NoteType.Hold => holdNotePool,
            NoteType.Sky => skyNotePool,
            NoteType.Arc => arcNotePool,*/
            _ => throw new System.Exception()
        };
    }

    public void InitPools(FumenData fumen)
    {
        NormalPool.Init(fumen.NormalPoolCount);
        CirclePool.Init(fumen.CirclePoolCount);
        SlidePool.Init(fumen.SlidePoolCount);
        FlickPool.Init(fumen.FlickPoolCount);
        HoldPool.Init(fumen.HoldPoolCount);
        SkyPool.Init(fumen.SkyPoolCount);
        ArcPool.Init(fumen.ArcPoolCount);
        LinePool.Init(fumen.LinePoolCount);
    }

#if UNITY_EDITOR
    [ContextMenu("Apply PoolCount")]
    void ApplyPoolCount()
    {
        var inGameManager = FindAnyObjectByType<InGameManager>();
        var fumenData = inGameManager.FumenData;
        fumenData.SetPoolCount(new int[8] {
            NormalPool.MaxUseCount,
            CirclePool.MaxUseCount,
            SlidePool.MaxUseCount,
            FlickPool.MaxUseCount,
            HoldPool.MaxUseCount,
            SkyPool.MaxUseCount,
            ArcPool.MaxUseCount,
            LinePool.MaxUseCount,
        });
        UnityEditor.EditorUtility.SetDirty(fumenData);
    }
#endif
}
