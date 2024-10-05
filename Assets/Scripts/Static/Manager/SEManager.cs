using CriWare;
using UnityEngine;

public class SEManager : SingletonMonoBehaviour<SEManager>
{
    [SerializeField] CriAtomSource source;

    public void SetVolume(float value)
    {
        source.volume = value;
    }

    public void PlaySE(SEType type)
    {
        source.cueSheet = type.ToString();
        source.Play(type.ToString());
    }
}

public enum SEType
{
    my1,
    my2,
    start_freeze,
    ti,
}