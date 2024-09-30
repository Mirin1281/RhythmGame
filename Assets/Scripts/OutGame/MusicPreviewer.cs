using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusicPreviewer : MonoBehaviour
{
    [SerializeField] CriAtomSource source;
    CancellationTokenSource cts = new();
    readonly float previewVolume = 0.2f;
    
    public async UniTask MusicPreview(MusicData musicData)
    {
        cts?.Cancel();
        cts = new();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cts.Token);
        var token = cts.Token;

        source.cueSheet = musicData.SheetName;
        source.cueName = musicData.CueName;
        source.startTime = Mathf.RoundToInt(musicData.PreviewStart * 1000f);
        await MyUtility.LoadCueSheetAsync(musicData.SheetName);

        float time = musicData.PreviewStart;
        FadeInAsync(0.5f, token).Forget();
        
        while(token.IsCancellationRequested == false)
        {
            if(time > musicData.PreviewEnd)
            {
                await FadeOutAsync(0.5f, token);
                time = musicData.PreviewStart;
                FadeInAsync(0.5f, token).Forget();
            }
            time += Time.deltaTime;
            await UniTask.Yield(token);
        }
    }

    public async void Stop(float fadeTime = 0f)
    {
        cts?.Cancel();
        await FadeOutAsync(fadeTime, destroyCancellationToken);
        source.Stop();
    }

    async UniTask FadeInAsync(float time, CancellationToken token)
    {
        source.Play();
        source.volume = 0;
        var outQuad = new Easing(0, previewVolume, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            source.volume = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(token);
        }
        source.volume = previewVolume;
    }
    async UniTask FadeOutAsync(float time, CancellationToken token)
    {
        var outQuad = new Easing(source.volume, 0f, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            source.volume = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(token);
        }
        source.volume = 0f;
        source.Stop();
    }
}
