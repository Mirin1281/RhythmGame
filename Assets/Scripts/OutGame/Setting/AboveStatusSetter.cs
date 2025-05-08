using UnityEngine;
using UnityEngine.UI;

public class AboveStatusSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsComboAbove);
    }

    public void OnToggle()
    {
        RhythmGameManager.Setting.IsComboAbove = toggle.isOn;
        SEManager.Instance.PlaySE(SEType.ti);
    }
}
