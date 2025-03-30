using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace NoteCreating
{
    public class DevelopmentInitializer : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool developmentMode = true;
        [SerializeField] FumenData editorFumenData;
        public FumenData EditorFumenData => editorFumenData;
#else
        bool developmentMode = false;
#endif
        [SerializeField] Image darkImage;
        [Space(20)]
        [SerializeField] bool isMirror;
        [SerializeField] bool isDark;
        [SerializeField] float noteSeRate = 1;
        [SerializeField, Tooltip("5～10の範囲")] int speed = 7;
        [SerializeField] float offset;
        [SerializeField] bool isEarphone;
        [Space(20)]
        [SerializeField] bool isAuto = true;
        [SerializeField] bool showDebugRange;

        public void Init()
        {
            SetMirror(developmentMode);
            SetDarkMode(developmentMode);

            if (developmentMode)
            {
                RhythmGameManager.Setting.NoteSEVolume = noteSeRate * RhythmGameManager.Setting.NoteSEVolume;
                SEManager.Instance.SetCategoryVolume(ConstContainer.NoteSECategory, RhythmGameManager.GetNoteVolume());

                RhythmGameManager.Setting.Speed = speed * 10;

                RhythmGameManager.Setting.Offset = Mathf.RoundToInt(offset * 1000);
                if (isEarphone) RhythmGameManager.Setting.Offset -= 100;
                FindAnyObjectByType<NoteInput>().IsAuto = isAuto;
                FindAnyObjectByType<Judgement>().ShowDebugRange = showDebugRange;
            }
            else
            {
                FindAnyObjectByType<NoteInput>().IsAuto = RhythmGameManager.Setting.IsAutoPlay;
            }
            darkImage = null;
        }

        public async UniTask<FumenData> LoadFumenData()
        {
            FumenData fumenData = null;
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
            return fumenData;
        }
        void SetMirror(bool developmentMode)
        {
            var cameraMirrors = FindObjectsByType<CameraMirror>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var mainCameraMirror = cameraMirrors.First(c => c.gameObject.name == "Main Camera");
            var effectCameraMirror = cameraMirrors.First(c => c.gameObject.name == "EffectCamera");
            var negativeCameraMirror = cameraMirrors.First(c => c.gameObject.name == "NegativeCamera");

            if (developmentMode)
            {
                RhythmGameManager.Setting.IsMirror = isMirror;
                mainCameraMirror.IsInvert = isMirror;
                effectCameraMirror.IsInvert = isMirror;
                negativeCameraMirror.IsInvert = isMirror;
            }
            else
            {
                bool isMirror = RhythmGameManager.Setting.IsMirror;
                mainCameraMirror.IsInvert = isMirror;
                effectCameraMirror.IsInvert = isMirror;
                negativeCameraMirror.IsInvert = isMirror;
            }
        }

        void SetDarkMode(bool developmentMode)
        {
            bool l_isDark;
            if (developmentMode)
            {
                l_isDark = isDark;
                darkImage.gameObject.SetActive(l_isDark);
                RhythmGameManager.Setting.IsDark = l_isDark;
            }
            else
            {
                l_isDark = RhythmGameManager.Setting.IsDark;
                darkImage.gameObject.SetActive(l_isDark);
            }

            var clearCamera = GameObject.Find("ClearCamera").GetComponent<Camera>();
            clearCamera.backgroundColor = l_isDark ? Color.white : Color.black;
            SlideNote.BaseAlpha = l_isDark ? 0.5f : 0.3f; // ダークモードだと色が薄くなるので調整
            ArcNote.HoldingAlpha = l_isDark ? 0.7f : 0.55f;
            ArcNote.NotHoldingAlpha = l_isDark ? 0.4f : 0.3f;
            Line.BaseAlpha = l_isDark ? 1.2f : 1f;
        }
    }
}