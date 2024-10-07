using TMPro;
using UnityEngine;

public class Speed3DSetter : MonoBehaviour
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
        if(isAdd && RhythmGameManager.SettingSpeed3D >= 100f
        || isAdd == false && RhythmGameManager.SettingSpeed3D <= 50f) return;
        if(isAdd)
        {
            RhythmGameManager.SettingSpeed3D++;
        }
        else
        {
            RhythmGameManager.SettingSpeed3D--;
        }
        SetText();
        SEManager.Instance.PlaySE(SEType.ti);
    }

    void SetText()
    {
        speedTmpro.SetText("{0:0.0}", RhythmGameManager.SettingSpeed3D / 10f);
    }
}
