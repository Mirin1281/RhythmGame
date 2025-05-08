using UnityEngine;
using UnityEngine.UI;

public class AccuracySetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsShowAccuracy);
    }

    public void OnToggle()
    {
        RhythmGameManager.Setting.IsShowAccuracy = toggle.isOn;
        SEManager.Instance.PlaySE(SEType.ti);
    }
}
