using UnityEngine;
using UnityEditor;

public class PropertyDrawerHelper
{
    Rect position;
    readonly SerializedProperty property;
    readonly float startX;
    readonly float startWidth;
    int indentLevel;

    public PropertyDrawerHelper(Rect position, SerializedProperty property, float contentHeight = 20)
    {
        this.position = new Rect(position.x, position.y, position.width, contentHeight);
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
    public void SetY(float addHeight = 0, bool reset = true)
    {
        if (reset)
        {
            SetX(0);
            SetWidth(startWidth * (1f - IndentDepth));
        }
        position.y += position.height + addHeight;
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
}