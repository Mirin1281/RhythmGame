using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField] InputManager inputManager;

    public void Pause()
    {
        Metronome.Instance.Pause();
        inputManager.EnableInput = false;
        gameObject.SetActive(true);
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        inputManager.EnableInput = true;
        Metronome.Instance.Resume();
    }
}
