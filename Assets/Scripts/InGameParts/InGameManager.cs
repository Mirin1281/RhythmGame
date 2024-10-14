using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

public class InGameManager : MonoBehaviour
{
    [SerializeField] FumenData editorFumenData;
    [SerializeField] Metronome metronome;
    [SerializeField] NotePoolManager notePoolManager;
    FumenData fumenData;
    bool isLoaded;

    public FumenData FumenData => fumenData;
    public bool IsLoaded => isLoaded;

    void Awake()
    {
        Init().Forget();
    }
    async UniTask Init()
    {
        isLoaded = false;
        if(RhythmGameManager.Instance != null && string.IsNullOrEmpty(RhythmGameManager.FumenName) == false)
        {
            fumenData = await Addressables.LoadAssetAsync<FumenData>(RhythmGameManager.FumenName);
        }
        else
        {
            fumenData = editorFumenData;
        }
        
        if(fumenData.Start3D)
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

        // プール数の設定
        notePoolManager.SetPoolCount(FumenData);

        // 音楽データをロード
        await MyUtility.LoadCueSheetAsync(fumenData.MusicSelectData.SheetName);

        await UniTask.DelayFrame(2);

        // アークは最初が高負荷なので予め生成しておく
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
        
        isLoaded = true;
        await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
        metronome.Play();
    }
}
