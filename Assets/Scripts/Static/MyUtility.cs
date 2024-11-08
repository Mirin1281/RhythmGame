using System;
using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class MyUtility
{
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

    public static Vector2 GetRotatedPos(Vector2 pos, float deg, Vector2 centerPos = default)
    {
        var cos = Mathf.Cos(deg * Mathf.Deg2Rad);
        var sin = Mathf.Sin(deg * Mathf.Deg2Rad);
        return new Vector2(
            centerPos.x + (pos.x - centerPos.x) * cos - (pos.y - centerPos.y) * sin,
            centerPos.y + (pos.x - centerPos.x) * sin + (pos.y - centerPos.y) * cos);
    }

    public static UniTask LoadCueSheetAsync(string cueSheetName, string acbName = null)
    {
        CriAtom.AddCueSheetAsync(cueSheetName, (acbName ?? cueSheetName) + ".acb", "");
        return UniTask.WaitWhile(() => CriAtom.CueSheetsAreLoading);
    }
}
