using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NoteGenerating;
using CriWare;

public class MusicButtonManager : MonoBehaviour
{
    [SerializeField] SelectMusicButton selectButtonPrefab;
    [SerializeField] MusicPreviewer previewer;
    [SerializeField] Image illustImage;
    [SerializeField] TMP_Text titleTmpro;
    [SerializeField] TMP_Text composerTmpro;
    [SerializeField] TMP_Text illustratorTmpro;
    [SerializeField] List<MusicMasterData> datas;
    List<MusicMasterData> sortedDatas;
    readonly List<SelectMusicButton> buttons = new();
    int selectedIndex = -1;

    public event Action<MusicMasterData> OnOtherSelect;

    void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < sortedDatas.Count; i++)
        {
            var selectButton = Instantiate(selectButtonPrefab, this.transform);
            buttons.Add(selectButton);
        }
        Sort(RhythmGameManager.Difficulty);

        UniTask.Void(async () => 
        {
            await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
            if(RhythmGameManager.SelectedIndex >= 0)
            {
                buttons[RhythmGameManager.SelectedIndex].OnSelect();
            }
            else
            {
                buttons[0].OnSelect();
            }
        });
    }
    void OnDestroy()
    {
        OnOtherSelect = null;
    }

    public void Sort(Difficulty diff)
    {
        // selectedIndexがソート後にどこにいくかを調べるため
        string beforeName = selectedIndex == -1 ? null : buttons[selectedIndex].MusicName;
        
        sortedDatas = datas.Where(d => d.GetFumenData(diff) != null) // 難易度が存在するものを選定
            .OrderBy(d => d.GetFumenData(diff).Level) // レベルの数値で並べ替え
            .ThenBy(d => d.MusicData.InternalMusicName) // 楽曲名で並べ替え
            .ToList();
        for(int i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];
            if(sortedDatas.Count <= i)
            {
                b.gameObject.SetActive(false);
            }
            else
            {
                b.gameObject.SetActive(true);
                b.SetData(sortedDatas[i], i);
            }

            if(beforeName == b.MusicName)
            {
                // 1画面8楽曲まで表示、1つあたり120ほどの高さ
                float toPosY = i < 4 ? 0 : 120f * (i - 6f);
                transform.DOLocalMoveY(toPosY, 0.1f).SetEase(Ease.OutQuart);
                buttons[selectedIndex].Deselect();
                selectedIndex = i;
                buttons[i].Pop();
            }
        }
    }

    public void NotifyByChild(int index)
    {
        MusicMasterData data = sortedDatas[index];
        if(selectedIndex == index)
        {
            RhythmGameManager.Instance.MusicMasterData = data;
            previewer.Stop(0.5f);
            FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
            Debug.Log($"楽曲名: {data.MusicData.MusicName}\n" +
                $"難易度: {RhythmGameManager.Difficulty} {data.GetFumenData(RhythmGameManager.Difficulty).Level}");
        }
        else
        {
            if(selectedIndex != -1)
            {
                previewer.Stop(0f);
                CriAtom.RemoveCueSheet(sortedDatas[selectedIndex].MusicData.SheetName);
                buttons[selectedIndex].Deselect();
            }
            previewer.MusicPreview(data.MusicData).Forget();
            SEManager.Instance.PlaySE(SEType.my1);
            illustImage.sprite = data.Illust;
            titleTmpro.SetText(data.MusicData.MusicName);
            composerTmpro.SetText(data.MusicData.ComposerName);
            illustratorTmpro.SetText(data.IllustratorName);

            OnOtherSelect?.Invoke(data);

            selectedIndex = index;
            RhythmGameManager.SelectedIndex = selectedIndex;
        }
    }
}
