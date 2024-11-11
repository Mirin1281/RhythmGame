using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    public void Pause()
    {
        Metronome.Instance.Pause();
        gameObject.SetActive(true);
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        Metronome.Instance.Resume();
    }
}
