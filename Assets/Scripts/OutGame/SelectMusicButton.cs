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

    public string MusicName => musicName;

    public void SetData(MusicSelectData data, int index)
    {
        this.index = index;
        diff = RhythmGameManager.Difficulty;
        if(data == null)
        {
            musicName = null;
            return;
        }
        levelTmpro.SetText(data.GetFumenLevel(diff).ToString());
        nameTmpro.SetText(data.MusicName.ToString());
        illustImage.sprite = data.Illust;
        musicName = data.MusicName;
        plateImage.color = GetDifficultyColor();
    }

    Color GetDifficultyColor(byte alpha = 150)
    {
        return diff switch
        {
            Difficulty.Normal => new Color32(0, 160, 140, alpha),
            Difficulty.Hard => new Color32(0, 60, 160, alpha),
            Difficulty.Extra => new Color32(160, 0, 100, alpha),
            _ => throw new System.Exception()
        };
    }

    public void OnSelect()
    {
        var creator = transform.parent.GetComponent<MusicButtonManager>();
        creator.NotifyInput(index);
        Pop();
    }
    
    public void Pop()
    {
        plateImage.color = GetDifficultyColor(alpha: 230);
        transform.DOLocalMoveX(300f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        plateImage.color = GetDifficultyColor();
        transform.DOLocalMoveX(460f, 0.2f).SetEase(Ease.OutBack);
    }
}
