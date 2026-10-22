using UnityEngine;

public class DotAligner : MonoBehaviour
{
	public float Width = 2f;

	public bool Even;

	public void Start()
	{
		Align(((Component)this).transform, Width, Even);
	}

	public static void Align(Transform target, float width, bool even)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < target.childCount; i++)
		{
			if (((Component)target.GetChild(i)).gameObject.activeSelf)
			{
				num++;
			}
		}
		float num2;
		float num3;
		if (even)
		{
			num2 = (0f - width) * (float)(num - 1) / 2f;
			num3 = width;
		}
		else if (num > 1)
		{
			num2 = (0f - width) / 2f;
			num3 = width / (float)(num - 1);
		}
		else
		{
			num2 = 0f;
			num3 = 0f;
		}
		int num4 = 0;
		for (int j = 0; j < target.childCount; j++)
		{
			Transform child = target.GetChild(j);
			if (((Component)child).gameObject.activeSelf)
			{
				((Component)child).transform.localPosition = new Vector3(num2 + (float)num4 * num3, 0f, 0f);
				num4++;
			}
		}
	}
}
