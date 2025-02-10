using System.Linq;
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
            {
                RhythmGameManager.SettingIsMirror = isMirror;
                var cameraScalers = FindObjectsByType<CameraScaler>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                var mainCameraScaler = cameraScalers.First(c => c.gameObject.name == "Main Camera");
                var effectCameraScaler = cameraScalers.First(c => c.gameObject.name == "EffectCamera");
                var negativeCameraScaler = cameraScalers.First(c => c.gameObject.name == "NegativeCamera");

                Vector3 scale = new Vector3(isMirror ? -1 : 1, 1, 1);
                mainCameraScaler.ScreenScale = scale;
                effectCameraScaler.ScreenScale = scale;
                negativeCameraScaler.ScreenScale = scale;
            }
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
                    new(-2, new Lpb(0), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(2, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(0, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    }, RhythmGameManager.Speed);
                arc.SetActive(false);
            }

            await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
            metronome.Play(fumenData);

            titleTmpro = null;
        }
    }
}