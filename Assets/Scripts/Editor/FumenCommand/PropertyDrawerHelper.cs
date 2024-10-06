using UnityEngine;
using UnityEditor;

public class PropertyDrawerHelper
{
    Rect position;
    readonly SerializedProperty property;
    readonly float startX;
    public readonly float StartWidth;
    public PropertyDrawerHelper(Rect position, SerializedProperty property, float contentHeight = 20)
    {
        this.position = position;
        this.property = property;
        startX = position.x;
        StartWidth = position.width;
        this.position.height = contentHeight;
    }

    public float SetX(float x) => position.x = startX + x;

    /// <summary>
    /// 次の行へ移ります。通常position.xとwidthはリセットされます
    /// </summary>
    public float SetY(float addHeight = 0, bool reset = true)
    {
        if(reset)
        {
            SetX(0);
            SetWidth(StartWidth);
        }
        position.y += position.height + addHeight;
        return position.y;
    }

    public float SetWidth(float width) => position.width = width;

    /// <summary>
    /// フィールドを描画します。返り値としてSerializedPropertyを受け取れます
    /// </summary>
    public SerializedProperty PropertyField(string fieldName, bool drawLabel = true)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if(drawLabel)
        {
            EditorGUI.PropertyField(position, prop);
        }
        else
        {
            EditorGUI.PropertyField(position, prop, GUIContent.none);
        }
        return prop;
    }

    public void LabelField(string name)
    {
        EditorGUI.LabelField(position, name);
    }
}