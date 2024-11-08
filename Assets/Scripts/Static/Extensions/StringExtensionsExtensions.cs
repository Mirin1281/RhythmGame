using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    public static Vector2 ToVector2(this string self)
    {
        var elements = self.Trim('(', ')').Split(',');
        var result = Vector2.zero;
        var elementCount = Mathf.Min(elements.Length, 2);

        for (var i = 0; i < elementCount; i++)
        {
            float.TryParse(elements[i], out float value);
            result[i] = value;
        }
        return result;
    }

    public static Vector3 ToVector3(this string self)
    {
        var elements = self.Trim('(', ')').Split(',');
        var result = Vector3.zero;
        var elementCount = Mathf.Min(elements.Length, 3);

        for (var i = 0; i < elementCount; i++)
        {
            float.TryParse(elements[i], out float value);
            result[i] = value;
        }
        return result;
    }
}
