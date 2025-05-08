using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SelectMusicButton : MonoBehaviour
{
    [SerializeField] TMP_Text levelTmpro;
    [SerializeField] TMP_Text nameTmpro;
    [SerializeField] Image plateImage;
    [SerializeField] Image illustImage;
    int index;
    string musicName;
    Difficulty diff;
    bool selected;

    public string MusicName => musicName;

    public void SetData(MusicSelectData data, int index)
    {
        this.index = index;
        diff = RhythmGameManager.Difficulty;
        if (data == null)
        {
            musicName = null;
            return;
        }
        levelTmpro.SetText(data.GetFumenLevel(diff).ToString());
        nameTmpro.SetText(data.MusicName.ToString());
        illustImage.sprite = data.Illust;
        musicName = data.MusicName;
        plateImage.color = GetDifficultyColor(150);
    }

    Color GetDifficultyColor(byte alpha)
    {
        return diff switch
        {
            Difficulty.Normal => new Color32(0, 170, 150, alpha),
            Difficulty.Hard => new Color32(30, 120, 210, alpha),
            Difficulty.Extra => new Color32(160, 0, 100, alpha),
            _ => throw new System.Exception()
        };
    }

    public void OnSelect()
    {
        var creator = transform.parent.GetComponent<MusicButtonManager>();
        creator.NotifyInput(index);
        Pop();
        selected = true;
        UpdateTextsColor();
    }

    public void Pop()
    {
        plateImage.color = GetDifficultyColor(alpha: 230);
        transform.DOLocalMoveX(300f, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    public void Deselect()
    {
        plateImage.color = GetDifficultyColor(150);
        transform.DOLocalMoveX(451.5f, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
        selected = false;
        UpdateTextsColor();
    }

    public void UpdateTextsColor()
    {
        levelTmpro.color = GetColor();
        nameTmpro.color = GetColor();
    }

    public void SetDark(bool enable)
    {
        UpdateTextsColor();
    }

    Color GetColor()
    {
        return (RhythmGameManager.Setting.IsDark, selected) switch
        {
            (false, false) => Color.white,
            (false, true) => Color.white,
            (true, true) => Color.black,
            (true, false) => new Color(0.1f, 0.1f, 0.1f, 1),
        };
    }
}
