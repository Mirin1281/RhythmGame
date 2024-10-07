using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField] Metronome metronome;

    public void Pause()
    {
        metronome.Pause();
        gameObject.SetActive(true);
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        metronome.Resume();
    }
}
