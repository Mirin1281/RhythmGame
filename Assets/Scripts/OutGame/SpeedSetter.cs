using TMPro;
using UnityEngine;

public class SpeedSetter : MonoBehaviour
{
    [SerializeField] TMP_Text speedTmpro;
    [SerializeField] bool isAdd;

    void Start()
    {
        var holdableButton = GetComponent<HoldableButton>();
        holdableButton.OnInput += SetSpeed;
        SetText();
    }

    void SetSpeed()
    {
        int speed = RhythmGameManager.Setting.Speed;
        if (isAdd && speed >= 100f
        || isAdd == false && speed <= 50f) return;

        int a;
        if (isAdd)
        {
            a = 1;
        }
        else
        {
            a = -1;
        }
        RhythmGameManager.Setting.Speed += a;
        SetText();
        SEManager.Instance.PlaySE(SEType.ti);
    }

    void SetText()
    {
        speedTmpro.SetText("{0:0.0}", RhythmGameManager.Setting.Speed / 10f);
    }
}
