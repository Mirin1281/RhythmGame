using System.Collections.Generic;
using UnityEngine;
using CriWare;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using UnityEngine.UI;

public class MusicMasterCreator : MonoBehaviour
{
    [SerializeField] CriAtomSource source;
    [SerializeField] Image illustImage;
    [SerializeField] SelectMusicButton selectButtonPrefab;
    [SerializeField] List<MusicMasterData> datas;
    List<MusicMasterData> sortedDatas;
    readonly List<SelectMusicButton> buttons = new();
    int selectedIndex = -1;
    readonly float previewVolume = 0.3f;

    void Start()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        sortedDatas = datas.OrderBy(d => d.FumenData.Level)
        .ThenBy(d => d.MusicData.InternalMusicName)
        .ToList();

        for(int i = 0; i < sortedDatas.Count; i++)
        {
            var selectButton = Instantiate(selectButtonPrefab, this.transform);
            selectButton.SetData(sortedDatas[i], i);
            selectButton.gameObject.SetActive(true);
            buttons.Add(selectButton);
        }
    }

    CancellationTokenSource cts = new();
    public void Select(int index)
    {
        if(selectedIndex == index)
        {
            RhythmGameManager.Instance.MusicMasterData = sortedDatas[index];
            cts?.Cancel();
            FadeOutAsync(0.5f, destroyCancellationToken).Forget();
            FadeLoadSceneManager.Instance.LoadScene(1f, "InGame");
        }
        else
        {
            MusicPreview(selectedIndex, index).Forget();
            illustImage.sprite = sortedDatas[index].Illust;

            if(selectedIndex != -1)
            {
                buttons[selectedIndex].Deselect();
            }
            selectedIndex = index;
        }
    }

    async UniTask MusicPreview(int beforeIndex, int index)
    {
        if(beforeIndex != -1)
        {
            source.Stop();
        }

        cts?.Cancel();
        cts = new();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cts.Token);
        var token = cts.Token;

        var musicData = sortedDatas[index].MusicData;
        source.cueSheet = musicData.SheetName;
        source.cueName = musicData.CueName;
        source.startTime = Mathf.RoundToInt(musicData.PreviewStart * 1000f);

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
