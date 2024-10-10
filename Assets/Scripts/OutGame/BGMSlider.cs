using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IVolumeChangable
{
    void ChangeVolume(float value);
}

public class BGMSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text tmpro;
    [SerializeField] GameObject musicPlayable;
    IVolumeChangable volumeChanable;

    void Awake()
    {
        var val = RhythmGameManager.SettingBGMVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
        if(musicPlayable)
        {
            volumeChanable = musicPlayable.GetComponent<IVolumeChangable>();
        }
    }

    void SetText(float value)
    {
        tmpro.SetText("{0:0.00}", value);
    }

    public void OnValueChange()
    {
        var val = slider.value;
        SetText(val);
        RhythmGameManager.SettingBGMVolume = val;
        volumeChanable.ChangeVolume(RhythmGameManager.GetBGMVolume());
    }
}
