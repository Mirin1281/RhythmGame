using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInversableCommand
{
    /// <summary>
    /// 反転をトグル式に変化させます
    /// </summary>
    void SetToggleInverse();
}
