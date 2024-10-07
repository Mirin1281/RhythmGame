using TMPro;
using UnityEngine;

public class SpeedSetter : MonoBehaviour
{
    [SerializeField] TMP_Text speedTmpro;
    [SerializeField] bool isAdd;

    void Start()
    {
        var holdableButton = GetComponent<HoldableButton>();
        holdableButton.OnInput += SetSpeed;
        SetText();
    }

    void SetSpeed()
    {
        if(isAdd && RhythmGameManager.SettingSpeed >= 100f
        || isAdd == false && RhythmGameManager.SettingSpeed <= 50f) return;
        if(isAdd)
        {
            RhythmGameManager.SettingSpeed++;
        }
        else
        {
            RhythmGameManager.SettingSpeed--;
        }
        SetText();
        SEManager.Instance.PlaySE(SEType.ti);
    }

    void SetText()
    {
        speedTmpro.SetText("{0:0.0}", RhythmGameManager.SettingSpeed / 10f);
    }
}
