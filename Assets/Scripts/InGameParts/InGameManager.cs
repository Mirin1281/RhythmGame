using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

public class InGameManager : MonoBehaviour
{
    [SerializeField] MusicMasterData masterData;
    [SerializeField] Metronome metronome;
    [SerializeField] NotePoolManager notePoolManager;
    [SerializeField] 
    public MusicMasterData MasterData => masterData;
    public FumenData FumenData => masterData.GetFumenData();
    public MusicData MusicData => masterData.MusicData;

    async UniTask Awake()
    {
        if(RhythmGameManager.Instance != null && RhythmGameManager.Instance.MusicMasterData != null)
        {
            masterData = RhythmGameManager.Instance.MusicMasterData;
        }
        if(masterData.GetFumenData(RhythmGameManager.Difficulty).Start3D)
        {
            var rendererShower = GameObject.FindAnyObjectByType<RendererShower>(FindObjectsInactive.Include);
            var CameraMover = GameObject.FindAnyObjectByType<CameraMover>(FindObjectsInactive.Include);
            rendererShower.ShowLaneAsync(0).Forget();
            CameraMover.Move(new Vector3(0f, 7f, -6.5f), new Vector3(25f, 0f, 0f),
                CameraMoveType.Absolute,
                0,
                EaseType.None
            );
        }

        // 初期化 //
        notePoolManager.SetPoolCount(FumenData);
        await MyUtility.LoadCueSheetAsync(MusicData.SheetName);
        await UniTask.DelayFrame(2);

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
