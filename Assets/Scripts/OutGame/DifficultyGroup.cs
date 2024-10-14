using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;

public class DifficultyGroup : MonoBehaviour
{
    [SerializeField] DifficultyButton buttonPrefab;
    [SerializeField] MusicButtonManager buttonManager;
    [SerializeField] MusicPreviewer previewer;
    List<DifficultyButton> buttons;
    Difficulty selectedDiff = Difficulty.None;
    MusicSelectData selectData;

    public event Action<Difficulty> OnChangeDifficulty;

    void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        buttonManager.OnOtherSelect += UpdateButton;

        buttons = new(3);
        for(int i = 0; i < 3; i++)
        {
            var b = Instantiate(buttonPrefab, this.transform);
            Difficulty diff = (Difficulty)(i + 1);
            b.Init(diff);
            buttons.Add(b);
        }
        GetButton(RhythmGameManager.Difficulty).OnSelect();
    }
    void OnDestroy()
    {
        OnChangeDifficulty = null;
    }

    DifficultyButton GetButton(Difficulty difficulty)
    {
        return buttons[(int)difficulty - 1];
    }

    void UpdateButton(MusicSelectData data)
    {
        this.selectData = data;
        buttons[0].SetLevel(data.GetFumenLevel(Difficulty.Normal));
        buttons[1].SetLevel(data.GetFumenLevel(Difficulty.Hard));

        bool isExistExtra = string.IsNullOrEmpty(data.ExtraFumenAddress) == false;
        buttons[2].gameObject.SetActive(isExistExtra);
        if(isExistExtra)
        {
            buttons[2].SetLevel(data.GetFumenLevel(Difficulty.Extra));
        }
    }

    public void NotifyByChild(Difficulty difficulty)
    {
        RhythmGameManager.Difficulty = difficulty;
        if(selectedDiff == difficulty)
        {
            StartGame(selectData, difficulty);
        }
        else
        {
            if(selectedDiff != Difficulty.None)
            {
                GetButton(selectedDiff).Deselect();
            }
            buttonManager.SortByDifficulty(difficulty);
            OnChangeDifficulty?.Invoke(difficulty);
            selectedDiff = difficulty;
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
