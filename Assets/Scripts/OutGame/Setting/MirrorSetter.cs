using UnityEngine;
using UnityEngine.UI;

public class MirrorSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsMirror);
    }

    public void OnToggle()
    {
        RhythmGameManager.Setting.IsMirror = toggle.isOn;
        SEManager.Instance.PlaySE(SEType.ti);
    }
}
