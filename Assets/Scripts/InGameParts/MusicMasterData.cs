using UnityEngine;
using NoteGenerating;

[CreateAssetMenu(
    fileName = "Master_",
    menuName = "ScriptableObject/Master",
    order = 2)
]
public class MusicMasterData : ScriptableObject
{
    [SerializeField] MusicData musicData;
    [Space(20)]
    [SerializeField] FumenData normalFumenData;
    [SerializeField] FumenData hardFumenData;
    [SerializeField] FumenData extraFumenData;
    [Space(20)]
    [SerializeField] Sprite illust;
    [SerializeField] string illustratorName;

    public MusicData MusicData => musicData;
    public FumenData GetFumenData(Difficulty difficulty = Difficulty.None)
    {
        if(difficulty == Difficulty.None)
        {
            difficulty = RhythmGameManager.Difficulty;
        }
        return difficulty switch
        {
            Difficulty.Normal => normalFumenData,
            Difficulty.Hard => hardFumenData,
            Difficulty.Extra => extraFumenData,
            _ => throw new System.Exception()
        };
    }
    public Sprite Illust => illust;
    public string IllustratorName => illustratorName;
}