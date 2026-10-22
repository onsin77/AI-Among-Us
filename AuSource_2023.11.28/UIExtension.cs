using UnityEngine;

public static class UIExtension
{
	public static Vector3[] GetCorners(this RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		return array;
	}

	public static float MaxY(this RectTransform rectTransform)
	{
		return rectTransform.GetCorners()[1].y;
	}

	public static float MinY(this RectTransform rectTransform)
	{
		return rectTransform.GetCorners()[0].y;
	}

	public static float MaxX(this RectTransform rectTransform)
	{
		return rectTransform.GetCorners()[2].x;
	}

	public static float MinX(this RectTransform rectTransform)
	{
		return rectTransform.GetCorners()[0].x;
	}
}
