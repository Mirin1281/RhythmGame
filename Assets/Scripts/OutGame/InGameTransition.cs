using UnityEngine;

public class InGameTransition : MonoBehaviour
{
    [SerializeField] string fumenAddress;
    [SerializeField] Difficulty difficulty = Difficulty.Normal;

    public void Transition2InGame()
    {
        RhythmGameManager.Difficulty = difficulty;
        RhythmGameManager.SelectedIndex = 0;
        var manager = FindAnyObjectByType<MusicSelectManager>();
        manager.StartGame(fumenAddress);
    }
}
