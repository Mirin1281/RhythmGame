using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectMusicButton : MonoBehaviour
{
    [SerializeField] TMP_Text levelTmpro;
    [SerializeField] TMP_Text nameTmpro;
    [SerializeField] Image image;
    MusicMasterData data;

    public void SetData(MusicMasterData data)
    {
        this.data = data;
        levelTmpro.SetText(data.FumenData.Level.ToString());
        nameTmpro.SetText(data.MusicData.MusicName.ToString());
        image.sprite = data.Illust;
    }

    public void StartGame()
    {
        RhythmGameManager.Instance.MusicMasterData = data;
        FadeLoadSceneManager.Instance.LoadScene(1f, "InGame");
    }
}
