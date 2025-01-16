using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using NoteCreating;

public class NoteVolumeSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text tmpro;
    CancellationTokenSource cts;

    void Awake()
    {
        var val = RhythmGameManager.SettingNoteSEVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
    }
    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    void SetText(float val)
    {
        tmpro.SetText(val.ToString("0.00"));
    }

    public void OnValueChange()
    {
        var val = slider.value;
        SetText(val);
        RhythmGameManager.SettingNoteSEVolume = val;
        SEManager.Instance.SetCategoryVolume(ConstContainer.NoteSECategory, RhythmGameManager.GetNoteVolume());

        cts?.Cancel();
        cts = new();
        var token = cts.Token;

        UniTask.Void(async () =>
        {
            await MyUtility.WaitSeconds(0.3f, token);
            SEManager.Instance.PlaySE(SEType.my2);
        });
    }

    public void SetInteractable(bool enabled)
    {
        slider.interactable = enabled;
        SetColor(slider.fillRect);
        SetColor(slider.handleRect);
        SetText(enabled ? RhythmGameManager.SettingNoteSEVolume : 0);


        void SetColor(RectTransform rect)
        {
            var image = rect.GetComponent<Image>();
            if (enabled)
            {
                image.color = Color.black;
            }
            else
            {
                image.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }
    }
}
