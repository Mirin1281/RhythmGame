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
        source.Play(type.ToString());
    }

    public void PlayNoteSE(SEType type)
    {
        source.Play(ToStringFromEnum(type));
    }

    static string ToStringFromEnum(SEType value)
    {
        return value switch
        {
            SEType.my2 => "my2",
            SEType.my2_low => "my2_low",

            _ => throw new System.InvalidOperationException(),
        };
    }
}

public enum SEType
{
    None,
    my1,
    my2,
    my2_low,
    start_freeze,
    ti,
}