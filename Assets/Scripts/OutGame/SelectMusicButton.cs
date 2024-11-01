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

    public string MusicName => musicName;

    public void SetData(MusicSelectData data, int index)
    {
        this.index = index;
        levelTmpro.SetText(data.GetFumenLevel(RhythmGameManager.Difficulty).ToString());
        nameTmpro.SetText(data.MusicName.ToString());
        illustImage.sprite = data.Illust;
        musicName = data.MusicName;
    }

    public void OnSelect()
    {
        var creator = transform.parent.GetComponent<MusicButtonManager>();
        creator.NotifyInput(index);
        Pop();
    }
    
    public void Pop()
    {
        plateImage.color = new Color32(0, 0, 0, 230);
        transform.DOLocalMoveX(300f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        plateImage.color = new Color32(90, 90, 90, 150);
        transform.DOLocalMoveX(460f, 0.2f).SetEase(Ease.OutBack);
    }
}
