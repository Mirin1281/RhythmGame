using UnityEngine;
using UnityEditor;

public class DrawerHelper
{
    Rect position;
    readonly SerializedProperty property;
    readonly float startX;
    readonly float startWidth;
    int indentLevel;

    public const int Height = 20;

    public DrawerHelper(Rect position, SerializedProperty property)
    {
        this.position = new Rect(position.x, position.y, position.width, Height);
        this.property = property;
        this.startX = position.x;
        this.startWidth = position.width;
    }

    public float GetX() => position.x;
    public void SetX(float x)
    {
        position.x = startX + x + IndentDepth * startWidth;
    }

    /// <summary>
    /// 幅の割合からX座標を設定します。0だと左端、0.8程度で右端に描画されます
    /// </summary>
    public void SetXAsWidth(float x)
    {
        SetX(x * startWidth);
    }

    /// <summary>
    /// 次の行へ移ります。通常、X座標と幅はリセットされます
    /// </summary>
    public void SetY(float overrideHeight = 0, bool reset = true)
    {
        if (reset)
        {
            SetX(0);
            SetWidth(startWidth * (1f - IndentDepth));
        }
        if (overrideHeight == 0)
        {
            position.y += position.height;
        }
        else
        {
            position.y += overrideHeight;
        }
    }

    public float GetWidth() => position.width;
    public void SetWidth(float width) => position.width = width;

    float IndentDepth => 0.05f * indentLevel;
    public void SetIndentLevel(bool increment)
    {
        indentLevel += increment ? 1 : -1;
        SetX(0);
        SetWidth(startWidth * (1f - IndentDepth));
    }


    public SerializedProperty PropertyField(string fieldName, bool drawLabel = true, string overrideName = null)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if (prop.propertyType is SerializedPropertyType.Vector2 or SerializedPropertyType.Vector2Int
            or SerializedPropertyType.Vector3 or SerializedPropertyType.Vector3Int)
        {
            return VectorField(prop, drawLabel, overrideName);
        }
        if (drawLabel)
        {
            if (overrideName == null)
            {
                EditorGUI.PropertyField(position, prop);
            }
            else
            {
                EditorGUI.PropertyField(position, prop, new GUIContent(overrideName));
            }
        }
        else
        {
            EditorGUI.PropertyField(position, prop, GUIContent.none);
        }
        return prop;
    }
    public SerializedProperty PropertyField(float width, string fieldName, bool drawLabel = true, string overrideName = null)
    {
        float w = position.width;
        SetWidth(width);
        var prop = PropertyField(fieldName, drawLabel, overrideName);
        SetWidth(w);
        return prop;
    }

    SerializedProperty VectorField(SerializedProperty vectorProperty, bool drawLabel = true, string overrideName = null)
    {
        if (drawLabel)
        {
            if (overrideName == null)
            {
                LabelField(vectorProperty.displayName);
            }
            else
            {
                LabelField(overrideName);
            }
        }

        SetX(EditorGUIUtility.labelWidth + 5);
        SetWidth(GetWidth() - EditorGUIUtility.labelWidth - 5);
        EditorGUI.PropertyField(position, vectorProperty, GUIContent.none);
        SetX(startX);
        SetWidth(GetWidth());
        return vectorProperty;
    }

    public void LabelField(string name)
    {
        EditorGUI.LabelField(position, name);
    }
    public void LabelField(float width, string name)
    {
        float w = position.width;
        SetWidth(width);
        EditorGUI.LabelField(position, name);
        SetWidth(w);
    }

    public void DrawLine(Color? color = null, int weight = 1)
    {
        EditorGUI.DrawRect(new Rect(position.x, position.y, position.width, weight), color ?? new Color(0.7f, 0.7f, 0.7f));
    }

    public void DrawBox(Rect position, Color color, float alpha = 0.1f)
    {
        var originalColor = GUI.color;

        // Alpha値を小さくしないと文字が見えないので下げる
        GUI.color = new Color(color.r, color.g, color.b, alpha);
        var style = new GUIStyle
        {
            normal =
            {
                background = Texture2D.whiteTexture
            }
        };
        GUI.Box(position, string.Empty, style);

        GUI.color = originalColor;
    }

    public int GetArrayElementIndex()
    {
        static bool IsArrayElement(SerializedProperty property)
        {
            string path = property.propertyPath;
            return path.Contains(".Array.data[");
        }
        if (IsArrayElement(property) == false)
        {
            Debug.LogError("Property is not ArrayElement");
            return -1;
        }
        string path = property.propertyPath;

        int startIndex = path.LastIndexOf('[');
        int endIndex = path.LastIndexOf(']');
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string indexString = path.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (int.TryParse(indexString, out int index))
            {
                return index;
            }
        }

        Debug.LogError("Unknown Error");
        return -1;
    }
}