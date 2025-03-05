using UnityEngine;

public static class ColorExtensions
{
    /// <summary>
    /// 透明度を設定した色を返します。ex. Color.red.Alpha(0.5f)
    /// </summary>
    public static Color Alpha(this in Color self, float alpha)
    {
        return new Color(self.r, self.g, self.b, alpha);
    }
}
