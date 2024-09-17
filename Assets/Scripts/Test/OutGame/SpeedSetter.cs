using TMPro;
using UnityEngine;

public class SpeedSetter : MonoBehaviour
{
    [SerializeField] SettingButton button;
    [SerializeField] TMP_Text speedTmpro;

    void Start()
    {
        button.OnInput += SetSpeed;
        speedTmpro.SetText(RhythmGameManager.Speed.ToString("0.0"));
    }

    void SetSpeed(bool isUp)
    {
        RhythmGameManager.SetSpeed(isUp);
        speedTmpro.SetText(RhythmGameManager.Speed.ToString("0.0"));
    }
}
