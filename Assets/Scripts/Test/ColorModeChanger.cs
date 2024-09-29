using UnityEngine;
using UnityEngine.UI;

public class ColorModeChanger : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [SerializeField] Image backImage;

    [SerializeField] SpriteRenderer judgeLine;
    [SerializeField] SpriteRenderer judgeLine3D;

    [SerializeField] MeshRenderer lane3D;

    [SerializeField] SpriteRenderer judgeLine3D_Sky;

    void Start()
    {
        SetColor(1);
    }

    /// <summary>
    /// 0～1で、0だとダークモード、1だとホワイトモードになります
    /// </summary>
    void SetColor(float value)
    {
        float v = value;
        float k = 1 - v;
        mainCamera.backgroundColor = new Color(v, v, v);

        backImage.color = new Color32(255, 255, 255, (byte)v.Ease(5, 30, 1, EaseType.Linear));
        judgeLine.color = new Color(k, k, k);
        judgeLine3D.color = new Color(k, k, k);
        lane3D.sharedMaterial.color = new Color(v, v, v);
        judgeLine3D_Sky.color = new Color(k, k, k);
    }
}
