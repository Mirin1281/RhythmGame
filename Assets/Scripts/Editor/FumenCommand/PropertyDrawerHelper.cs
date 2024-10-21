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
        startX = position.x;
        startWidth = position.width;
    }

    /// <summary>
    /// X座標を設定します。返り値を受けることもできます
    /// </summary>
    public float SetX(float x)
        => position.x = startX + IndentDepth * StartWidth + x;

    /// <summary>
    /// 全体幅の割合を指定してX座標を設定します。0だとデフォルト、0.8程度で右端に描画されます
    /// </summary>
    public float SetXAsWidth(float x)
        => SetX(x * Width);

    /// <summary>
    /// 次の行へ移ります。通常position.xとwidthはリセットされます
    /// </summary>
    public float SetY(float addHeight = 0, bool reset = true)
    {
        if(reset)
        {
            SetX(0);
            SetWidth(startWidth * (1f - IndentDepth));
        }
        position.y += position.height + addHeight;
        return position.y;
    }

    public float SetWidth(float width) => position.width = width;

    public int IndentLevel
    {
        get => indentLevel;
        set
        {
            indentLevel = value;
            SetX(0);
            SetWidth(startWidth * (1f - IndentDepth));
        }
    }

    public float StartWidth => startWidth;

    public float Width => position.width;

    float IndentDepth => 0.05f * indentLevel;

    /// <summary>
    /// フィールドを描画します。返り値としてSerializedPropertyを受け取れます
    /// </summary>
    public SerializedProperty PropertyField(string fieldName, bool drawLabel = true, string overrideName = null)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if(drawLabel)
        {
            if(overrideName == null)
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
    /// <summary>
    /// フィールドを描画します。返り値としてSerializedPropertyを受け取れます
    /// </summary>
    public SerializedProperty PropertyField(float width, string fieldName, bool drawLabel = true)
    {
        float w = position.width;
        SetWidth(width);
        var prop = property.FindPropertyRelative(fieldName);
        if(drawLabel)
        {
            EditorGUI.PropertyField(position, prop);
        }
        else
        {
            EditorGUI.PropertyField(position, prop, GUIContent.none);
        }
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

    public void DrawLine()
    {
        EditorGUI.DrawRect(
            new Rect(position.x, position.y, position.width, 1),
            new Color(0.7f, 0.7f, 0.7f));
    }
}