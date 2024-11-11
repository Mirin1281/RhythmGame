using System;
using UnityEngine;

public class DifficultyGroup : MonoBehaviour
{
    [SerializeField] MusicSelectManager sceneManager;
    [SerializeField] DifficultyButton buttonPrefab;
    [SerializeField] MusicButtonManager buttonManager;
    
    DifficultyButton[] buttons;
    Difficulty selectedDiff = Difficulty.None;
    MusicSelectData selectData;

    public event Action<Difficulty> OnChangeDifficulty;

    public void Init()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        buttonManager.OnOtherSelect += UpdateButton;

        buttons = new DifficultyButton[3];
        for(int i = 0; i < 3; i++)
        {
            var b = Instantiate(buttonPrefab, this.transform);
            buttons[i] = b;
            Difficulty diff = (Difficulty)(i + 1);
            b.Init(diff);
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
        ActiveDiffButton(Difficulty.Normal);
        ActiveDiffButton(Difficulty.Hard);
        ActiveDiffButton(Difficulty.Extra);
        

        void ActiveDiffButton(Difficulty difficulty)
        {
            int index = (int)difficulty - 1;
            bool isExist = string.IsNullOrEmpty(data.GetFumenAddress(difficulty)) == false;
            buttons[index].gameObject.SetActive(isExist);
            if(isExist)
            {
                buttons[index].SetLevel(data.GetFumenLevel(difficulty));
            }
        }
    }

    public void NotifyByChild(Difficulty difficulty)
    {
        RhythmGameManager.Difficulty = difficulty;
        if(selectedDiff == difficulty)
        {
            sceneManager.StartGame(selectData, difficulty);
            return;
        }

        if(selectedDiff != Difficulty.None)
        {
            GetButton(selectedDiff).Deselect();
        }
        buttonManager.SortByDifficulty(difficulty);
        OnChangeDifficulty?.Invoke(difficulty);
        selectedDiff = difficulty;
    }
}
