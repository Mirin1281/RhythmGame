using System.Collections.Generic;
using NoteGenerating;
using UnityEngine;

public class DifficultyGroup : MonoBehaviour
{
    [SerializeField] MusicButtonManager creator;
    [SerializeField] MusicPreviewer previewer;
    [SerializeField] DifficultyButton buttonPrefab;
    readonly List<DifficultyButton> buttons = new();
    Difficulty selectedDiff = Difficulty.None;
    MusicMasterData masterData;

    void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        creator.OnOtherSelect += UpdateButton;

        for(int i = 0; i < 3; i++)
        {
            var b = Instantiate(buttonPrefab, this.transform);
            Difficulty diff = (Difficulty)(i + 1);
            b.Init(diff);
            buttons.Add(b);
        }
        GetButton(RhythmGameManager.Difficulty).OnSelect();
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
            RhythmGameManager.Instance.MusicMasterData = masterData;
            previewer.Stop(0.5f);
            FadeLoadSceneManager.Instance.LoadScene(0.5f, "InGame", 0.5f, Color.white);
            Debug.Log($"楽曲名: {masterData.MusicData.MusicName}\n" +
                $"難易度: {difficulty} {masterData.GetFumenData(difficulty).Level}");
        }
        else
        {
            creator.Sort(difficulty);

            if(selectedDiff != Difficulty.None)
            {
                GetButton(selectedDiff).Deselect();
            }
            selectedDiff = difficulty;
        }
    }
}
