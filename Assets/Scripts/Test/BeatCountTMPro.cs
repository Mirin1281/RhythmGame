using TMPro;
using UnityEngine;

public class BeatCountTMPro : MonoBehaviour
{
    [SerializeField] TMP_Text tmpro;
    [SerializeField] Metronome metronome;

    void Awake()
    {
        metronome.OnBeat += SetText;
    }

    void SetText(int beatCount, float _)
    {
        tmpro.SetText("{0}", beatCount - RhythmGameManager.DefaultWaitOnAction);
    }
}
