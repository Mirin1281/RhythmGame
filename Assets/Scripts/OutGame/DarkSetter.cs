using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DarkSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsDark);
    }

    public void OnToggle()
    {
        UniTask.Void(async () =>
        {
            await RhythmGameManager.SetDarkModeAsync(toggle.isOn);
            SEManager.Instance.PlaySE(SEType.ti);
        });
    }
}
