using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BGMSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text tmpro;

    void Awake()
    {
        var val = RhythmGameManager.Instance.BGMVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
    }

    void SetText(float val)
    {
        tmpro.SetText(val.ToString("0.00"));
    }

    public void OnValueChange()
    {
        var val = slider.value;
        RhythmGameManager.Instance.BGMVolume = val;
        SetText(val);
    }
}
