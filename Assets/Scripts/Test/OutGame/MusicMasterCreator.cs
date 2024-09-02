using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MusicMasterCreator : MonoBehaviour
{
    [SerializeField] SelectMusicButton selectButtonPrefab;
    [SerializeField] List<MusicMasterData> datas;

    void Start()
    {
        datas.Sort((d1, d2) => d1.FumenData.Level - d2.FumenData.Level);
        for(int i = 0; i < datas.Count; i++)
        {
            var selectButton = Instantiate(selectButtonPrefab, this.transform);
            selectButton.SetData(datas[i]);
            selectButton.gameObject.SetActive(true);
        }
    }
}
