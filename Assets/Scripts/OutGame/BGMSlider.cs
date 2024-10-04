using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IVolumeChanable
{
    void ChangeVolume(float value);
}

public class BGMSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text tmpro;
    [SerializeField] GameObject musicPlayable;
    IVolumeChanable volumeChanable;

    void Awake()
    {
        var val = RhythmGameManager.Instance.RawBGMVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
        if(musicPlayable)
        {
            volumeChanable = musicPlayable.GetComponent<IVolumeChanable>();
        }
    }

    void SetText(float val)
    {
        tmpro.SetText(val.ToString("0.00"));
    }

    public void OnValueChange()
    {
        var val = slider.value;
        SetText(val);
        RhythmGameManager.Instance.BGMVolume = val;
        val = RhythmGameManager.Instance.BGMVolume;
        volumeChanable.ChangeVolume(val);
    }
}
