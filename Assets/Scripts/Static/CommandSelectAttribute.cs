using UnityEngine;

/// <summary>
/// CommandBase内のCommandDataフィールドにアタッチしてください
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field)]
public class CommandSelectAttribute : PropertyAttribute
{
    public CommandSelectAttribute()
    {

    }
}
