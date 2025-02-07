using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 図形をかたどって弾を配置する際の座標を求めるクラス
/// </summary>
public static class DrawPolygonCulculator
{
    const int Deg360 = 360;

    public static IReadOnlyList<Vector2> GetPolygonPoses(
        int corner,
        int sideCount,
        float size = 6f,
        int density = 1,
        float rotationDeg = 0
    )
    {
        sideCount--;
        rotationDeg += 90;
        // 四角形の時だけ回転角が特殊
        if (corner == 4)
        {
            rotationDeg += 45;
        }
        var poses = new Vector2[sideCount * corner];
        var count = 0;

        for (int j = 0; j < corner; j++)
        {
            var currentDot = new Vector2(
                Cos(j * Deg360 / corner + rotationDeg),
                Sin(j * Deg360 / corner + rotationDeg)
            );
            var nextDot = new Vector2(
                Cos((j + density) * Deg360 / corner + rotationDeg),
                Sin((j + density) * Deg360 / corner + rotationDeg)
            );
            for (int i = 0; i < sideCount; i++)
            {
                poses[count] = size * new Vector2(
                    Mathf.Lerp(currentDot.x, nextDot.x, (float)i / sideCount),
                    Mathf.Lerp(currentDot.y, nextDot.y, (float)i / sideCount)
                );
                count++;
            }
        }
        return poses;
    }

    /// <summary>
    /// n芒星で、交差部分がないように座標を生成します
    /// </summary>
    public static IReadOnlyList<Vector2> GetPolygonInNothingPoses(
        int corner,
        int sideCount,
        float size = 6f,
        int density = 2,
        float rotationDeg = 0,
        float opt = 1
    // densityを3以上にしたい場合はoptを微調整する必要あり
    )
    {
        if (density < 2) Debug.LogWarning("densityが1以下です");
        sideCount--;
        rotationDeg += 90;
        var length = Mathf.Tan(Mathf.PI / corner) * opt;
        var poses = new Vector2[corner * sideCount * 2];
        var count = 0;

        for (int j = 0; j < corner; j++)
        {
            var currentDot = new Vector2(
                Cos(j * Deg360 / corner + rotationDeg),
                Sin(j * Deg360 / corner + rotationDeg)
            );
            // ある点からみて左側の頂点
            var leftNextDot = new Vector2(
                Cos((j + density) * Deg360 / corner + rotationDeg),
                Sin((j + density) * Deg360 / corner + rotationDeg)
            );
            var rightNextDot = new Vector2(
                Cos((j - density) * Deg360 / corner + rotationDeg),
                Sin((j - density) * Deg360 / corner + rotationDeg)
            );

            DrawBarrageLine(leftNextDot, false);
            DrawBarrageLine(rightNextDot, true);

            void DrawBarrageLine(Vector2 nextDot, bool isOneShift)
            {
                var currentToNextDir = GetAim(currentDot, nextDot);
                var insideDot = currentDot + new Vector2(
                    Cos(currentToNextDir) * length,
                    Sin(currentToNextDir) * length
                );
                for (int i = 0; i < sideCount; i++)
                {
                    var _i = isOneShift ? i + 1 : i;
                    poses[count] = size * new Vector2(
                        Mathf.Lerp(currentDot.x, insideDot.x, (float)_i / sideCount),
                        Mathf.Lerp(currentDot.y, insideDot.y, (float)_i / sideCount)
                    );
                    count++;
                }
            }
        }
        return poses;
    }

    /// <summary>
    /// ベジェ曲線を描くような座標を求めます
    /// </summary>
    public static IReadOnlyList<Vector2> GetDrawBezierPoses(
        int count,
        Vector2 start,
        Vector2 end,
        Vector2 ctrl,
        float size = 1f
    )
    {
        var poses = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            var s = i / (count - 1f);
            poses[i] = size * new Vector2(
                (end.x * s * s) + (2 * s * (1 - s) * ctrl.x) + ((1 - s) * (1 - s) * start.x),
                (end.y * s * s) + (2 * s * (1 - s) * ctrl.y) + ((1 - s) * (1 - s) * start.y));
        }
        return poses;
    }

    /// <summary>
    /// ベジェ曲線(3次)
    /// </summary>
    public static IReadOnlyList<Vector2> GetDrawBezierPoses(
        int count,
        Vector2 start,
        Vector2 end,
        Vector2 ctrl1,
        Vector2 ctrl2,
        float size = 1f
    )
    {
        var poses = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            var s = i / (count - 1f);
            var u = 1 - s;
            poses[i] = size * new Vector2(
                (Pow(u, 3) * start.x) +
                (3 * u * u * s * ctrl1.x) +
                (3 * u * s * s * ctrl2.x) +
                (Pow(s, 3) * end.x),

                (Pow(u, 3) * start.y) +
                (3 * u * u * s * ctrl1.y) +
                (3 * u * s * s * ctrl2.y) +
                (Pow(s, 3) * end.y));
        }
        return poses;
    }

    public static float Sin(float deg) => Mathf.Sin(deg * Mathf.Deg2Rad);
    public static float Cos(float deg) => Mathf.Cos(deg * Mathf.Deg2Rad);

    public static float Pow(float value, int p)
    {
        float result = 1;
        for (int i = 0; i < p; i++)
        {
            result *= value;
        }
        return result;
    }

    /// <summary>
    /// p1からp2に向けての角度を度数法で返します
    /// </summary>
    public static float GetAim(Vector3 p1, Vector3 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        return Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
    }
}
