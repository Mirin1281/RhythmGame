using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using VertexType = NoteCreating.ArcCreateData.VertexType;

namespace NoteCreating
{
    public class InGameManager : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] FumenData editorFumenData;
        [SerializeField] bool isEarphone;
        [SerializeField] bool isMirror;
        [SerializeField] bool isDark;
        [SerializeField] float noteSeRate = 1;
        [SerializeField] int speed = 7;
#endif
        [SerializeField] TMP_Text titleTmpro;
        [SerializeField] Image darkImage;
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

            // 初期化(すぐできるもの) //
            RhythmGameManager.SpeedBase = 1f;

            metronome.GetComponent<IVolumeChangable>().ChangeVolume(RhythmGameManager.GetBGMVolume());

            SetDarkMode();
            SetMirror();

#if UNITY_EDITOR
            if (isEarphone) RhythmGameManager.Setting.Offset = -100;
            RhythmGameManager.Setting.NoteSEVolume = noteSeRate * RhythmGameManager.Setting.NoteSEVolume;
            SEManager.Instance.SetCategoryVolume(ConstContainer.NoteSECategory, RhythmGameManager.GetNoteVolume());
            RhythmGameManager.Setting.Speed = speed * 10;
#else
            FindAnyObjectByType<NoteInput>().IsAuto = RhythmGameManager.Setting.IsAutoPlay;
#endif

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

            // 初期化(fumenDataが必要なもの) //
            FindAnyObjectByType<PoolManager>().InitPools(fumenData);

            FindAnyObjectByType<Judgement>().Init(fumenData);

            titleTmpro.SetText($"<size=30><b>♪</b></size>{fumenData.MusicSelectData.MusicName}");

            RhythmGameManager.FumenName = MyUtility.GetFumenName(fumenData.MusicSelectData);

            // 音楽データをロード
            await MyUtility.LoadCueSheetAsync(fumenData.MusicSelectData.SheetName);

            metronome.Play(fumenData);

            if (fumenData.ArcPoolCount != 0)
            {
                // アークは最初が高負荷なので予め生成しておく
                var arcPool = FindAnyObjectByType<ArcNotePool>(FindObjectsInactive.Exclude);
                var arc = arcPool.GetNote();
                arc.SetPos(new Vector3(1000, 1000));
                var datas = new ArcCreateData[]
                {
                    new(-2, new Lpb(0), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(2, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(0, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                };
                await arc.CreateAsync(datas, RhythmGameManager.Speed);
                arc.SetActive(false);
            }

            titleTmpro = null;
            darkImage = null;
        }

        void SetMirror()
        {
            var cameraMirrors = FindObjectsByType<CameraMirror>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var mainCameraMirror = cameraMirrors.First(c => c.gameObject.name == "Main Camera");
            var effectCameraMirror = cameraMirrors.First(c => c.gameObject.name == "EffectCamera");
            var negativeCameraMirror = cameraMirrors.First(c => c.gameObject.name == "NegativeCamera");

#if UNITY_EDITOR
            RhythmGameManager.Setting.IsMirror = isMirror;
            mainCameraMirror.IsInvert = isMirror;
            effectCameraMirror.IsInvert = isMirror;
            negativeCameraMirror.IsInvert = isMirror;
#else
            bool isMirror = RhythmGameManager.Setting.IsMirror;
            mainCameraMirror.IsInvert = isMirror;
            effectCameraMirror.IsInvert = isMirror;
            negativeCameraMirror.IsInvert = isMirror;
#endif
        }

        void SetDarkMode()
        {
            var clearCamera = GameObject.Find("ClearCamera").GetComponent<Camera>();
            bool l_isDark = true;
#if UNITY_EDITOR
            l_isDark = isDark;
            darkImage.gameObject.SetActive(l_isDark);
            RhythmGameManager.Setting.IsDark = l_isDark;
#else
            l_isDark = RhythmGameManager.Setting.IsDark;
            darkImage.gameObject.SetActive(l_isDark);
#endif

            clearCamera.backgroundColor = l_isDark ? Color.white : Color.black;
            SlideNote.BaseAlpha = l_isDark ? 0.5f : 0.3f;
            ArcNote.HoldingAlpha = l_isDark ? 0.7f : 0.6f;
            ArcNote.NotHoldingAlpha = l_isDark ? 0.4f : 0.3f;
            Line.BaseAlpha = l_isDark ? 1.2f : 1f;
        }

#if UNITY_EDITOR
        public FumenData GetEditorFumenData() => editorFumenData;
#endif
    }
}