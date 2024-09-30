using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Metronome metronome;

    public void Pause()
    {
        metronome.Pause();
        gameObject.SetActive(true);
        canvas.enabled = true;
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        canvas.enabled = false;
        metronome.Resume();
    }
}
