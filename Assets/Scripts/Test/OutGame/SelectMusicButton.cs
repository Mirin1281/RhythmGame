using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectMusicButton : MonoBehaviour
{
    [SerializeField] TMP_Text levelTmpro;
    [SerializeField] TMP_Text nameTmpro;
    [SerializeField] Image plateImage;
    [SerializeField] Image illustImage;
    int index;

    public void SetData(MusicMasterData data, int index)
    {
        this.index = index;
        levelTmpro.SetText(data.FumenData.Level.ToString());
        nameTmpro.SetText(data.MusicData.MusicName.ToString());
        illustImage.sprite = data.Illust;
    }

    public void OnSelect()
    {
        var creator = transform.parent.GetComponent<MusicMasterCreator>();
        creator.Select(index);
        plateImage.color = Color.cyan;
    }

    public void Deselect()
    {
        plateImage.color = Color.white;
    }
}
