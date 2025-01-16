using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VertexType = NoteCreating.ArcCreateData.VertexType;

namespace NoteCreating
{
    public class InGameManager : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] FumenData editorFumenData;
#endif
        [SerializeField] TMP_Text titleTmpro;
        [SerializeField] bool isMirror;
        FumenData fumenData;

        public FumenData FumenData => fumenData;

        void Awake()
        {
            Init().Forget();
        }
        async UniTask Init()
        {
            // インスタンスの取得 //
            var metronome = Metronome.Instance;
            var poolManager = FindAnyObjectByType<PoolManager>();
            var judgement = FindAnyObjectByType<Judgement>();

            if (RhythmGameManager.Instance != null && RhythmGameManager.FumenReference != null)
            {
                fumenData = await Addressables.LoadAssetAsync<FumenData>(RhythmGameManager.FumenReference);
            }
            else
            {
#if UNITY_EDITOR
                fumenData = editorFumenData;
#endif
            }

            // 初期化 //
            poolManager.InitPools(fumenData);

            judgement.Init(fumenData);

            titleTmpro.SetText($"<size=30><b>♪</b></size>{fumenData.MusicSelectData.MusicName}");

            RhythmGameManager.FumenName = MyUtility.GetFumenName(fumenData.MusicSelectData);

            metronome.GetComponent<IVolumeChangable>().ChangeVolume(RhythmGameManager.GetBGMVolume());

#if UNITY_EDITOR
            if (isMirror)
                RhythmGameManager.SettingIsMirror = true;
#endif

            // 音楽データをロード
            await MyUtility.LoadCueSheetAsync(fumenData.MusicSelectData.SheetName);

            await UniTask.DelayFrame(2);

            if (fumenData.ArcPoolCount != 0)
            {
                // アークは最初が高負荷なので予め生成しておく
                var arcPool = FindAnyObjectByType<ArcNotePool>(FindObjectsInactive.Exclude);
                var arc = arcPool.GetNote();
                arc.SetPos(new Vector3(1000, 1000));
                await arc.CreateNewArcAsync(new ArcCreateData[]
                    {
                    new(-2, 0, VertexType.Auto, false, false, 0, 8),
                    new(2, 8, VertexType.Auto, false, false, 0, 8),
                    new(0, 8, VertexType.Auto, false, false, 0, 8),
                    }, 2);
                arc.SetActive(false);
            }

            await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
            metronome.Play(fumenData);

            titleTmpro = null;
        }
    }
}