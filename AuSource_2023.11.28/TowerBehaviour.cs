using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
	public float timer;

	public float frameTime = 0.2f;

	public SpriteRenderer circle;

	public SpriteRenderer middle1;

	public SpriteRenderer middle2;

	public SpriteRenderer outer1;

	public SpriteRenderer outer2;

	public void Update()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		if (timer < frameTime)
		{
			circle.color = Color.white;
			SpriteRenderer val = middle1;
			SpriteRenderer val2 = middle2;
			SpriteRenderer val3 = outer1;
			Color val4 = (outer2.color = Color.black);
			Color val5 = (val3.color = val4);
			Color color = (val2.color = val5);
			val.color = color;
		}
		else if (timer < 2f * frameTime)
		{
			SpriteRenderer val8 = middle1;
			Color color = (middle2.color = Color.white);
			val8.color = color;
			SpriteRenderer val9 = circle;
			SpriteRenderer val10 = outer1;
			Color val5 = (outer2.color = Color.black);
			color = (val10.color = val5);
			val9.color = color;
		}
		else if (timer < 3f * frameTime)
		{
			SpriteRenderer val12 = outer1;
			Color color = (outer2.color = Color.white);
			val12.color = color;
			SpriteRenderer val13 = middle1;
			SpriteRenderer val14 = middle2;
			Color val5 = (circle.color = Color.black);
			color = (val14.color = val5);
			val13.color = color;
		}
		else
		{
			timer = 0f;
		}
	}
}
