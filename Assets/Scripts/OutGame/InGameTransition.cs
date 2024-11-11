using UnityEngine;

public class InGameTransition : MonoBehaviour
{
    [SerializeField] string fumenAddress;

    public void Transition2InGame()
    {
        var manager = FindAnyObjectByType<MusicSelectManager>();
        manager.StartGame(fumenAddress);
    }
}
