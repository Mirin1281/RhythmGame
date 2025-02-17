using TMPro;
using UnityEngine;

public class OffsetSetter : MonoBehaviour
{
    [SerializeField] TMP_Text offsetTmpro;
    [SerializeField] bool isAdd;

    void Start()
    {
        var holdableButton = GetComponent<HoldableButton>();
        holdableButton.OnInput += SetOffset;
        SetText();
    }

    void SetOffset()
    {
        if (isAdd && RhythmGameManager.Offset >= 1f
        || isAdd == false && RhythmGameManager.Offset <= -1f) return;
        if (isAdd)
        {
            RhythmGameManager.Setting.Offset += 5;
        }
        else
        {
            RhythmGameManager.Setting.Offset -= 5;
        }
        SetText();
        SEManager.Instance.PlaySE(SEType.ti);
    }

    void SetText()
    {
        offsetTmpro.SetText(RhythmGameManager.Offset.ToString("0.000"));
    }
}
