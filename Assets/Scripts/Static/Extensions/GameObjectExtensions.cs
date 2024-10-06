using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// 子を全て別のオブジェクトへ移します
    /// </summary>
    public static void MoveChildren(this Transform self, Transform parentTs = null)
    {
        int childCount = self.childCount;
        for(int i = 0; i < childCount; i++)
        {
            self.GetChild(0).SetParent(parentTs);
        }
    }
}
