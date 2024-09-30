using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleCircleLayoutGroup : UIBehaviour, ILayoutGroup 
{
	public float radius = 100;
	public float offsetAngle;

	protected override void OnValidate()
	{
		base.OnValidate();
		Arrange();
	}

	// 要素数が変わると自動的に呼ばれるコールバック
	public void SetLayoutHorizontal(){}
	public void SetLayoutVertical()
	{
		Arrange();
	}

	void Arrange()
	{
		float splitAngle = 360 / transform.childCount;
		for (int i = 0; i < transform.childCount; i++)
        {
			var child = transform.GetChild (i) as RectTransform;
			float currentAngle = splitAngle * i + offsetAngle;
			child.anchoredPosition = radius * new Vector2(
				Mathf.Cos(currentAngle * Mathf.Deg2Rad), 
				Mathf.Sin(currentAngle * Mathf.Deg2Rad));
		}
	}
}