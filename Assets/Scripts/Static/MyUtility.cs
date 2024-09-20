using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;

public static class MyUtility
{
    public static GameObject GetPreviewObject()
    {
        GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(obj => obj.name == ConstContainer.DebugPreviewObjName)
            .FirstOrDefault();
        previewObj.SetActive(true);
        foreach(var child in previewObj.transform.OfType<Transform>().ToArray())
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        return previewObj;
    }

    public static UniTask WaitSeconds(float time, CancellationToken token)
    {
        if(time <= 0) return UniTask.CompletedTask;
        return UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: token);
    }

    /// <summary>
    /// 入力された座標が任意の四角形の中に入っているかを判定します
    /// </summary>
    /// <param name="rect">四角形</param>
    /// <param name="inputPos">入力座標</param>
    /// <param name="rotate">回転(ラジアン)</param>
    /// <returns></returns>
    public static bool IsPointInsideRectangle(Rect rect, Vector2 inputPos, float rotate = 0)
    {
        if(rotate == 0)
        {
            float dx = inputPos.x - rect.x;
            float dy = inputPos.y - rect.y;
            return Mathf.Abs(dx) <= rect.width / 2 && Mathf.Abs(dy) <= rect.height / 2;
        }
        else
        {
            // 点を四角形の中心に移動
            float dx = inputPos.x - rect.x;
            float dy = inputPos.y - rect.y;

            // 逆回転させる（四角形がrだけ回転しているので、点を-rだけ回転させる）
            float cosR = Mathf.Cos(-rotate);
            float sinR = Mathf.Sin(-rotate);

            // 回転行列を適用
            float rotatedX = dx * cosR - dy * sinR;
            float rotatedY = dx * sinR + dy * cosR;

            // 四角形の範囲内かどうかを確認（長辺はw、短辺はhの中心対称）
            return Mathf.Abs(rotatedX) <= rect.width / 2 && Mathf.Abs(rotatedY) <= rect.height / 2;
        }
    }
}
