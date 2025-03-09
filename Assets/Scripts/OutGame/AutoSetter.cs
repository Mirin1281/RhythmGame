using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AutoSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsAutoPlay);
    }

    public void OnToggle()
    {
        RhythmGameManager.Setting.IsAutoPlay = toggle.isOn;
        SEManager.Instance.PlaySE(SEType.ti);
    }
}
