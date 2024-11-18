using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSoundMute : MonoBehaviour
{
    bool isMute;
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite soundSprite;
    [SerializeField] Sprite muteSprite;
    [SerializeField] NoteVolumeSlider slider;

    void Awake()
    {
        if (RhythmGameManager.SettingIsNoteMute)
        {
            isMute = true;
            slider.SetInteractable(false);
            buttonImage.sprite = muteSprite;
        }
        else
        {
            isMute = false;
            slider.SetInteractable(true);
            buttonImage.sprite = soundSprite;
        }
    }

    public void ToggleSetMute()
    {
        if (isMute)
        {
            isMute = false;
            RhythmGameManager.SettingIsNoteMute = false;
            slider.SetInteractable(true);
            buttonImage.sprite = soundSprite;
        }
        else
        {
            isMute = true;
            RhythmGameManager.SettingIsNoteMute = true;
            slider.SetInteractable(false);
            buttonImage.sprite = muteSprite;
        }
    }
}
