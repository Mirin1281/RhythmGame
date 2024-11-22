using UnityEngine;

public class InGameTransition : MonoBehaviour
{
    [SerializeField] string fumenAddress;
    [SerializeField] Difficulty difficulty = Difficulty.Normal;

    public static void Transition2InGame(string fumenAddress, Difficulty difficulty)
    {
        RhythmGameManager.Difficulty = difficulty;
        RhythmGameManager.SelectedIndex = 0;
        var manager = FindAnyObjectByType<MusicSelectManager>();
        manager.StartGame(fumenAddress);
    }

    public void Transition2InGame()
    {
        Transition2InGame(fumenAddress, difficulty);
    }
}
