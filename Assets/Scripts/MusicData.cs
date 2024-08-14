using UnityEngine;

[CreateAssetMenu(
    fileName = "MusicData",
    menuName = "ScriptableObject/MusicData")
]
public class MusicData : ScriptableObject
{
    [SerializeField] AudioClip musicClip;
    [SerializeField] int bpm;

    [Header("’l‚ð‘å‚«‚­‚·‚é‚Æƒm[ƒc‚ª‚æ‚è‘‚­—Ž‚¿‚Ü‚·(‚æ‚èLate‚É‚È‚é)")]
    [SerializeField] float offset = 0;

    public int Bpm => bpm;
    public AudioClip MusicClip => musicClip;
    public float Offset => offset;
}
