using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class MusicButtonManager : MonoBehaviour
{
    [SerializeField] MusicSelectManager sceneManager;
    [SerializeField] SelectMusicButton buttonPrefab;
    [SerializeField] MusicMasterManagerData managerData;
    [SerializeField] InGameStarter gameStarter;

    SelectMusicButton[] buttons;
    MusicSelectData[] sortedDatas;
    int selectedIndex = -1;

    public event Action<MusicSelectData> OnOtherSelect;

    public async UniTask Init()
    {
        // エディタ上の子ボタンを消去
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // データの個数分ボタンを生成
        buttons = managerData.SelectDatas.Select(d => Instantiate(buttonPrefab, this.transform)).ToArray();

        SortByDifficulty(RhythmGameManager.Difficulty);

        await UniTask.Yield();
        buttons[Mathf.Clamp(RhythmGameManager.SelectedIndex, 0, int.MaxValue)].OnSelect();
    }
    void OnDestroy()
    {
        OnOtherSelect = null;
    }

    public void SortByDifficulty(Difficulty diff)
    {
        // 現在selectedIndexがソート後にどこにいくかを調べるため
        string beforeName = selectedIndex == -1 ? null : buttons[selectedIndex].MusicName;

        sortedDatas = managerData.SelectDatas.Where(d => d.GetFumenReference(diff).RuntimeKeyIsValid()) // 難易度が存在するものを選定
            .OrderBy(d => d.GetFumenLevel(diff)) // レベルの数値で並べ替え
            .ThenBy(d => d.MusicName) // 楽曲名で並べ替え
            .ToArray();

        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            if (sortedDatas.Length <= i)
            {
                b.gameObject.SetActive(false);
                b.SetData(null, i);
            }
            else
            {
                b.gameObject.SetActive(true);
                b.SetData(sortedDatas[i], i);
            }

            if (beforeName == b.MusicName && beforeName != null)
            {
                // 1画面8楽曲まで表示、1つあたり120ほどの高さ
                float toPosY = i < 4 ? 0 : 120f * (i - 6f);
                transform.DOLocalMoveY(toPosY, 0.1f).SetEase(Ease.OutQuart);
                buttons[selectedIndex].Deselect();
                selectedIndex = i;
                RhythmGameManager.SelectedIndex = i;
                buttons[i].Pop();
            }
        }
    }

    public void NotifyInput(int index)
    {
        if (sortedDatas.Length == 0)
        {
            RhythmGameManager.Difficulty = Difficulty.Normal;
            RhythmGameManager.SelectedIndex = -1;
            FindAnyObjectByType<MusicSelectManager>().Init();
            return;
        }
        if (index < 0 || index > sortedDatas.Length - 1)
        {
            index = 0;
        }
        MusicSelectData data = sortedDatas[index];
        RhythmGameManager.SelectedIndex = index;
        if (selectedIndex == index)
        {
            gameStarter.StartGame(data, RhythmGameManager.Difficulty);
            return;
        }


        if (selectedIndex != -1)
        {
            buttons[selectedIndex].Deselect();
        }
        OnOtherSelect?.Invoke(data);
        selectedIndex = index;
    }

    public void SetDark(bool enable)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetDark(enable);
        }
    }
}
