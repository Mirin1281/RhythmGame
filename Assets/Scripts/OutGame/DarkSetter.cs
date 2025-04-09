using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DarkSetter : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] MusicButtonManager musicButtonManager;
    [SerializeField] Material negativeMat;
    [SerializeField] Material invertMat;

    void Awake()
    {
        toggle.SetIsOnWithoutNotify(RhythmGameManager.Setting.IsDark);
    }

    public void InitOnAwake()
    {
        SetDark(RhythmGameManager.Setting.IsDark);
    }

    public void OnToggle()
    {
        SetDark(toggle.isOn);
        SEManager.Instance.PlaySE(SEType.ti);
    }

    void SetDark(bool enable)
    {
        RhythmGameManager.Setting.IsDark = enable;
        float value = enable ? 1 : 0;
        negativeMat.SetFloat("_BlendRate", value);
        invertMat.SetFloat("_Value", value);
        musicButtonManager.SetDark(enable);
    }
}
