using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/◆3Dレーンフェード"), System.Serializable]
    public class F_FadeLane: NoteGeneratorBase
    {
        enum FadeType
        {
            Show,
            Hide
        }
        [SerializeField] FadeType fadeType;
        [SerializeField] float time = 1f;

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            var rendererShower = GameObject.FindAnyObjectByType<RendererShower>(FindObjectsInactive.Exclude);
            if(fadeType == FadeType.Show)
            {
                rendererShower.ShowLaneAsync(time).Forget();
            }
            else if(fadeType == FadeType.Hide)
            {
                rendererShower.HideLaneAsync(time).Forget();
            }
        }
    }
}
