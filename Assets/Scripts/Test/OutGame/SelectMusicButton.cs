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
    public string MusicName;

    public void SetData(MusicMasterData data, int index)
    {
        this.index = index;
        levelTmpro.SetText(data.GetFumenData(RhythmGameManager.Difficulty).Level.ToString());
        nameTmpro.SetText(data.MusicData.MusicName.ToString());
        illustImage.sprite = data.Illust;
        MusicName = data.MusicData.MusicName;
    }

    public void OnSelect()
    {
        var creator = transform.parent.GetComponent<MusicButtonManager>();
        creator.NotifyByChild(index);
        Pop();
    }
    public void Pop()
    {
        plateImage.color = new Color32(0, 0, 0, 230);
        transform.DOLocalMoveX(500f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        plateImage.color = new Color32(90, 90, 90, 150);
        transform.DOLocalMoveX(626.5f, 0.2f).SetEase(Ease.OutBack);
    }
}
