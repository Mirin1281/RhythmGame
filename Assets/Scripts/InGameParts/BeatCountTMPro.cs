using TMPro;
using UnityEngine;

public class BeatCountTMPro : MonoBehaviour
{
    [SerializeField] TMP_Text tmpro;

    void Awake()
    {
        Metronome.Instance.OnBeat += SetText;
    }

    void SetText(int beatCount, float _)
    {
        tmpro.SetText("{0}", beatCount - RhythmGameManager.DefaultWaitOnAction);
    }
}
