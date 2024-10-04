using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IColorChangable
{
    void ChangeColor(float value);
}

public class ColorModeChanger : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] float value;
    [SerializeField] Camera mainCamera;

    [SerializeField] Image backImage;

    [SerializeField] SpriteRenderer judgeLine;
    [SerializeField] SpriteRenderer judgeLine3D;

    [SerializeField] MeshRenderer lane3D;

    [SerializeField] SpriteRenderer judgeLine3D_Sky;

    [SerializeField] Image pauseImage;
    [SerializeField] Material noteMaterial;
    [SerializeField] Material arcMaterial;

    void Awake()
    {
        SetColor(value);
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
        pauseImage.color = new Color(k, k, k);

        noteMaterial.SetFloat("_BlendRate", k);
        arcMaterial.SetFloat("_InverseBlendRate", k);
    }

    // シーン内の全てのインターフェースを取得します
    // 予定: IColorChangableを使って色変更
    static List<T> FindObjectOfInterfaces<T>() where T : class
	{
		var list = new List<T>();
		foreach (var component in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if(component is T t)
            {
				list.Add(t);
            }
		}
		return list;
	}
}
