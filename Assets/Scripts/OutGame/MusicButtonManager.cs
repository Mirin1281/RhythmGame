using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NoteGenerating;

public class MusicButtonManager : MonoBehaviour
{
    [SerializeField] SelectMusicButton selectButtonPrefab;
    [SerializeField] MusicMasterManagerData managerData;
    [SerializeField] MusicPreviewer previewer;
    List<MusicSelectData> sortedDatas;
    readonly List<SelectMusicButton> buttons = new();
    int selectedIndex = -1;

    public event Action<MusicSelectData> OnOtherSelect;

    void Awake()
    {
        Init().Forget();
    }

    async UniTask Init()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < managerData.SelectDatas.Length; i++)
        {
            var selectButton = Instantiate(selectButtonPrefab, this.transform);
            buttons.Add(selectButton);
        }
        SortByDifficulty(RhythmGameManager.Difficulty);

        await MyUtility.WaitSeconds(0.2f, destroyCancellationToken);
        if(RhythmGameManager.SelectedIndex >= 0)
        {
            buttons[RhythmGameManager.SelectedIndex].OnSelect();
        }
        else
        {
            buttons[0].OnSelect();
        }
    }
    void OnDestroy()
    {
        OnOtherSelect = null;
    }

    public void SortByDifficulty(Difficulty diff)
    {
        // selectedIndexがソート後にどこにいくかを調べるため
        string beforeName = selectedIndex == -1 ? null : buttons[selectedIndex].MusicName;
        
        sortedDatas = managerData.SelectDatas.Where(d => d.GetFumenAddress(diff) != null) // 難易度が存在するものを選定
            .OrderBy(d => d.GetFumenLevel(diff)) // レベルの数値で並べ替え
            .ThenBy(d => d.MusicName) // 楽曲名で並べ替え
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

    public void NotifyInput(int index)
    {
        MusicSelectData data = sortedDatas[index];
        if(selectedIndex == index)
        {
            StartGame(data, RhythmGameManager.Difficulty);
        }
        else
        {
            if(selectedIndex != -1)
            {
                buttons[selectedIndex].Deselect();
            }

            OnOtherSelect?.Invoke(data);
            selectedIndex = index;
            RhythmGameManager.SelectedIndex = selectedIndex;
        }
    }

    void StartGame(MusicSelectData selectData, Difficulty difficulty = Difficulty.None)
    {
        RhythmGameManager.FumenName = selectData.GetFumenAddress(difficulty);
        if(difficulty != Difficulty.None)
        {
            RhythmGameManager.Difficulty = difficulty;
        }
        previewer.Stop(0.5f).Forget();
        FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
        Debug.Log($"楽曲名: {selectData.MusicName}\n" +
            $"難易度: {RhythmGameManager.Difficulty} {selectData.GetFumenLevel(RhythmGameManager.Difficulty)}");
    }
}
