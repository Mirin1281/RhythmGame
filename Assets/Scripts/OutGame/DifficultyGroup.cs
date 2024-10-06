using System;
using System.Collections.Generic;
using NoteGenerating;
using UnityEngine;

public class DifficultyGroup : MonoBehaviour
{
    [SerializeField] DifficultyButton buttonPrefab;
    [SerializeField] MusicButtonManager buttonManager;
    [SerializeField] MusicPreviewer previewer;
    List<DifficultyButton> buttons;
    Difficulty selectedDiff = Difficulty.None;
    MusicMasterData masterData;

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

    void UpdateButton(MusicMasterData masterData)
    {
        this.masterData = masterData;
        buttons[0].SetLevel(masterData.GetFumenData(Difficulty.Normal).Level);
        buttons[1].SetLevel(masterData.GetFumenData(Difficulty.Hard).Level);

        FumenData extraFumen = masterData.GetFumenData(Difficulty.Extra);
        bool isExistExtra = extraFumen != null;
        buttons[2].gameObject.SetActive(isExistExtra);
        if(isExistExtra)
        {
            buttons[2].SetLevel(extraFumen.Level);
        }
    }

    public void NotifyByChild(Difficulty difficulty)
    {
        RhythmGameManager.Difficulty = difficulty;
        if(selectedDiff == difficulty)
        {
            StartGame(masterData, difficulty);
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

    void StartGame(MusicMasterData data, Difficulty difficulty = Difficulty.None)
    {
        RhythmGameManager.Instance.MusicMasterData = data;
        if(difficulty != Difficulty.None)
        {
            RhythmGameManager.Difficulty = difficulty;
        }
        previewer.Stop(0.5f);
        FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
        Debug.Log($"楽曲名: {data.MusicData.MusicName}\n" +
            $"難易度: {RhythmGameManager.Difficulty} {data.GetFumenData(RhythmGameManager.Difficulty).Level}");
    }
}
