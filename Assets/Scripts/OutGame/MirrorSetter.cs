using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MirrorSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.SettingIsMirror);
    }

    public void OnToggle()
    {
        RhythmGameManager.SettingIsMirror = toggle.isOn;
        SEManager.Instance.PlaySE(SEType.ti);
    }
}
