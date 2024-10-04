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
        var val = RhythmGameManager.Instance.RawSEVolume;
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
        SetText(val);
        RhythmGameManager.Instance.SEVolume = val;
        SEManager.Instance.SetVolume(RhythmGameManager.Instance.SEVolume);

        cts?.Cancel();
        cts = new();
        var token = cts.Token;

        UniTask.Void(async () => 
        {
            await MyUtility.WaitSeconds(0.3f, token);
            SEManager.Instance.PlaySE(SEType.my1);
        });
    }
}
