using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetChanger : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text text;

    void Start()
    {
        text.SetText(RhythmGameManager.Offset.ToString());
    }
    
    public void AddOffset()
    {
        RhythmGameManager.SettingOffset += 10;
        text.SetText(RhythmGameManager.Offset.ToString());
    }
    public void SubtractOffset()
    {
        RhythmGameManager.SettingOffset -= 10;
        text.SetText(RhythmGameManager.Offset.ToString());
    }
}
