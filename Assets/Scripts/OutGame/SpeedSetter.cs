using TMPro;
using UnityEngine;

public class SpeedSetter : MonoBehaviour
{
    [SerializeField] HoldableButton button;
    [SerializeField] TMP_Text speedTmpro;
    [SerializeField] bool isAdd;

    void Start()
    {
        button.OnInput += SetSpeed;
        speedTmpro.SetText(RhythmGameManager.Speed.ToString("0.0"));
    }

    void SetSpeed()
    {
        if(isAdd && RhythmGameManager.Speed >= 18f
        || isAdd == false && RhythmGameManager.Speed <= 10f) return;
        RhythmGameManager.SetSpeed(isAdd);
        speedTmpro.SetText(RhythmGameManager.Speed.ToString("0.0"));
    }
}
