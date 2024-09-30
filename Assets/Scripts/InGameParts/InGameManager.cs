using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

public class InGameManager : MonoBehaviour
{
    [SerializeField] MusicMasterData masterData;
    [SerializeField] Metronome metronome;
    public MusicMasterData MasterData => masterData;
    public FumenData FumenData => masterData.GetFumenData();
    public MusicData MusicData => masterData.MusicData;

    async UniTask Awake()
    {
        if(RhythmGameManager.Instance != null && RhythmGameManager.Instance.MusicMasterData != null)
        {
            masterData = RhythmGameManager.Instance.MusicMasterData;
        }

        // 初期化 //
        await MyUtility.LoadCueSheetAsync(MusicData.SheetName);

        var arcPool = FindAnyObjectByType<ArcNotePool>(FindObjectsInactive.Exclude);
        var arc = arcPool.GetNote();
        arc.SetPos(new Vector3(1000, 1000));
        await arc.CreateNewArcAsync(new ArcCreateData[]
            {
                new(new(-2, 4, 0), ArcVertexMode.Auto, false, false, 0, 8),
                new(new(-5, 3, 16), ArcVertexMode.Auto, true),
                new(new(-6, 2, 16), ArcVertexMode.Auto, false, false, 0, 8),
                new(new(-5, 1, 16), ArcVertexMode.Auto, true),
                new(new(-2, 0, 16), ArcVertexMode.Auto, true),
            }, 2);
        arc.SetActive(false);
        
        await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
        metronome.Play();
    }
}
