using CriWare;
using UnityEngine;

public class SEManager : SingletonMonoBehaviour<SEManager>
{
    [SerializeField] CriAtomSource source;

    /// <summary>
    /// カテゴリに属するSEの音量を調整します。カテゴリ名はAtomCraftで設定しています
    /// </summary>
    public void SetCategoryVolume(string categoryName, float value)
    {
        CriAtom.SetCategoryVolume(categoryName, value);
    }

    public void PlaySE(SEType type)
    {
        source.cueSheet = type.ToString();
        source.Play(type.ToString());
    }
}

public enum SEType
{
    None,
    my1,
    my2,
    start_freeze,
    ti,
}