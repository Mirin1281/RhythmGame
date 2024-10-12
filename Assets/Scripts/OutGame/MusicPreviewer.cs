using System.Collections.Generic;
using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusicPreviewer : MonoBehaviour, IVolumeChangable
{
    [SerializeField] CriAtomSource source;
    CancellationTokenSource cts = new();
    List<string> loadedCueSheetNames;

    // RemoveCueSheet()は重いので、ロードした曲を覚えてシーン移動時にまとめて削除
    void OnDestroy()
    {
        foreach(var n in loadedCueSheetNames)
        {
            CriAtom.RemoveCueSheet(n);
        }
    }

    void IVolumeChangable.ChangeVolume(float value)
    {
        source.volume = value;
    }
    
    public async UniTask MusicPreview(MusicData musicData)
    {
        loadedCueSheetNames ??= new();
        cts?.Cancel();
        cts = new();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cts.Token);
        var token = cts.Token;

        string sheet = musicData.SheetName;
        source.cueSheet = sheet;
        source.cueName = musicData.CueName;
        source.startTime = Mathf.RoundToInt(musicData.PreviewStart * 1000f);
        if(loadedCueSheetNames.Contains(sheet) == false)
        {
            await MyUtility.LoadCueSheetAsync(sheet);
            loadedCueSheetNames.Add(sheet);
        }

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

    public async UniTask Stop(float fadeTime = 0f)
    {
        cts?.Cancel();
        await FadeOutAsync(fadeTime, destroyCancellationToken);
        source.Stop();
    }

    async UniTask FadeInAsync(float time, CancellationToken token)
    {
        source.Play();
        source.volume = 0;
        float toVolume = RhythmGameManager.GetBGMVolume();
        var outQuad = new Easing(0, toVolume, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            source.volume = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, token);
        }
        source.volume = toVolume;
    }
    async UniTask FadeOutAsync(float time, CancellationToken token)
    {
        var outQuad = new Easing(source.volume, 0, time, EaseType.OutQuad);
        var t = 0f;
        while (t < time)
        {
            source.volume = outQuad.Ease(t);
            t += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, token);
        }
        source.volume = 0f;
        source.Stop();
    }
}
