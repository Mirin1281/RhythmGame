using Cysharp.Threading.Tasks;
using NoteGenerating;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

public class InGameManager : MonoBehaviour
{
    [SerializeField] FumenData editorFumenData;
    [SerializeField] Metronome metronome;
    [SerializeField] PoolManager notePoolManager;
    [SerializeField] Judgement judgement;
    [SerializeField] TMP_Text titleTmpro;
    FumenData fumenData;

    public FumenData FumenData => fumenData;

    void Awake()
    {
        Init().Forget();
    }
    async UniTask Init()
    {
        if(RhythmGameManager.Instance != null && string.IsNullOrEmpty(RhythmGameManager.FumenName) == false)
        {
            fumenData = await Addressables.LoadAssetAsync<FumenData>(RhythmGameManager.FumenName);
        }
        else
        {
            fumenData = editorFumenData;
        }
        
        // 初期化 //

        if(fumenData.Start3D)
        {
            var rendererShower = GameObject.FindAnyObjectByType<RendererShower>(FindObjectsInactive.Include);
            var cameraMover = GameObject.FindAnyObjectByType<CameraMover>(FindObjectsInactive.Include);
            rendererShower.ShowLaneAsync(0).Forget();
            cameraMover.Move(new Vector3(0f, 7f, -6.5f), new Vector3(25f, 0f, 0f),
                CameraMoveType.Absolute,
                0,
                EaseType.None
            );
        }

        notePoolManager.InitPools(fumenData);

        judgement.Init(fumenData);

        titleTmpro.SetText($"<size=30><b>♪</b></size>{fumenData.MusicSelectData.MusicName}");

        metronome.GetComponent<IVolumeChangable>().ChangeVolume(RhythmGameManager.GetBGMVolume());


        // 音楽データをロード
        await MyUtility.LoadCueSheetAsync(fumenData.MusicSelectData.SheetName);

        await UniTask.DelayFrame(2);

        if(fumenData.ArcPoolCount != 0)
        {
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
        }
        
        await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
        metronome.Play(fumenData);
    }
}
