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
        var val = RhythmGameManager.Setting.NoteSEVolume;
        slider.SetValueWithoutNotify(val);
        SetText(val);
    }
    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    void SetText(float value)
    {
        tmpro.SetText(Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnValueChange()
    {
        var val = slider.value;
        SetText(val);
        RhythmGameManager.Setting.NoteSEVolume = val;
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
        SetText(enabled ? RhythmGameManager.Setting.NoteSEVolume : 0);


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
