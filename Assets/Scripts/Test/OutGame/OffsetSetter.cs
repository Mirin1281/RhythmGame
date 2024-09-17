using TMPro;
using UnityEngine;

public class OffsetSetter : MonoBehaviour
{
    [SerializeField] SettingButton button;
    [SerializeField] TMP_Text offsetTmpro;

    void Start()
    {
        button.OnInput += SetOffset;
        offsetTmpro.SetText(RhythmGameManager.Offset.ToString("0.00"));
    }

    void SetOffset(bool isUp)
    {
        RhythmGameManager.SetOffset(isUp);
        offsetTmpro.SetText(RhythmGameManager.Offset.ToString("0.00"));
    }
}
