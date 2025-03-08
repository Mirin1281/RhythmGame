using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DarkSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] Material negativeMat;
    [SerializeField] Material invertMat;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsDark);
    }

    public void InitOnAwake()
    {
        float value = RhythmGameManager.Setting.IsDark ? 1 : 0;
        negativeMat.SetFloat("_BlendRate", value);
    }

    public void OnToggle()
    {
        UniTask.Void(async () =>
        {
            await RhythmGameManager.SetDarkModeAsync(toggle.isOn);
            float value = toggle.isOn ? 1 : 0;
            negativeMat.SetFloat("_BlendRate", value);
            invertMat.SetFloat("_Value", value);
            SEManager.Instance.PlaySE(SEType.ti);
        });
    }
}
