using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// 子オブジェクトの使用後、自動で破棄します
    /// </summary>
    public static void AutoDispose(this Transform self, CancellationToken token)
    {
        UniTask.Void(async () => 
        {
            await UniTask.Yield();
            await UniTask.WaitUntil(() => !IsAnyChildrenActive(self), cancellationToken: token);
            int childCount = self.childCount;
            for(int i = 0; i < childCount; i++)
            {
                self.GetChild(0).SetParent(null);
            }
            GameObject.Destroy(self.gameObject);
        });


        static bool IsAnyChildrenActive(Transform self)
        {
            for(int i = 0; i < self.childCount; i++)
            {
                if(self.GetChild(i).gameObject.activeSelf)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
