using NoteGenerating;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField] MusicMasterData masterData;
    public FumenData FumenData => masterData.FumenData;
    public MusicData MusicData => masterData.MusicData;

    public void SetMasterData(MusicMasterData masterData)
    {
        this.masterData = masterData;
    }
}
