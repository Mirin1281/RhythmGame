using UnityEngine;

[CreateAssetMenu(
    fileName = "MusicData",
    menuName = "ScriptableObject/MusicData")
]
public class MusicData : ScriptableObject
{
    [SerializeField] AudioClip musicClip;
    [SerializeField] int bpm;

    [Header("�l��傫������ƃm�[�c����葁�������܂�(���Late�ɂȂ�)")]
    [SerializeField] float offset = 0;

    public int Bpm => bpm;
    public AudioClip MusicClip => musicClip;
    public float Offset => offset;
}
