using CriWare;
using UnityEngine;

public class SEManager : SingletonMonoBehaviour<SEManager>
{
    [SerializeField] CriAtomSource source;

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
}