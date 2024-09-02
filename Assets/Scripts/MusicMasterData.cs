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
    [SerializeField] FumenData fumenData;
    [SerializeField] Sprite illust;

    public MusicData MusicData => musicData;
    public FumenData FumenData => fumenData;
    public Sprite Illust => illust;
}