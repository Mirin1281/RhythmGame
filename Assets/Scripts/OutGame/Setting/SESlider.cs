using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SESlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text tmpro;
    CancellationTokenSource cts;

    void Awake()
    {
        var val = RhythmGameManager.Setting.SEVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
    }

    void SetText(float value)
    {
        tmpro.SetText(Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnValueChange()
    {
        var val = slider.value;
        SetText(val);
        RhythmGameManager.Setting.SEVolume = val;
        SEManager.Instance.SetCategoryVolume(ConstContainer.SECategory, RhythmGameManager.GetSEVolume());

        cts?.Cancel();
        cts = new();
        var token = cts.Token;

        UniTask.Void(async () =>
        {
            await MyUtility.WaitSeconds(0.3f, token);
            SEManager.Instance.PlaySE(SEType.start_freeze);
        });
    }
}
