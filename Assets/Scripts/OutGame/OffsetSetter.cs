using TMPro;
using UnityEngine;

public class OffsetSetter : MonoBehaviour
{
    [SerializeField] HoldableButton button;
    [SerializeField] TMP_Text offsetTmpro;
    [SerializeField] bool isAdd;

    void Start()
    {
        button.OnInput += SetOffset;
        offsetTmpro.SetText(RhythmGameManager.Offset.ToString("0.00"));
    }

    void SetOffset()
    {
        if(isAdd && RhythmGameManager.Offset >= 1f
        || isAdd == false && RhythmGameManager.Offset <= -1f) return;
        RhythmGameManager.SetOffset(isAdd);
        offsetTmpro.SetText(RhythmGameManager.Offset.ToString("0.00"));
    }
}
