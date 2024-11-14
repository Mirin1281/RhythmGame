using UnityEngine;
using Random = System.Random;

public static class RandomExtensions
{
    public static float GetInt(this Random self, int min, int max)
    {
        return self.Next(min, max);
    }
    public static float GetFloat(this Random self, float min, float max)
    {
        return (float)self.NextDouble() * (max - min) + min;
    }

    public static Vector2 GetVector2(this Random self, Vector2 min, Vector2 max)
    {
        float x = GetFloat(self, min.x, max.x);
        float y = GetFloat(self, min.y, max.y);
        return new Vector2(x, y);
    }
    public static Vector3 GetVector3(this Random self, Vector3 min, Vector3 max)
    {
        float x = GetFloat(self, min.x, max.x);
        float y = GetFloat(self, min.y, max.y);
        float z = GetFloat(self, min.z, max.z);
        return new Vector3(x, y, z);
    }
}
