using UnityEngine;

public class PowerBar : MonoBehaviour
{
	public SpriteRenderer SegmentPrefab;

	public Sprite greenImage;

	public Sprite yellowImage;

	public Sprite redImage;

	public Sprite greenEmptyImage;

	public Sprite yellowEmptyImage;

	public Sprite redEmptyImage;

	public int numberGreen = 11;

	public int numberYellow = 6;

	public int numberRed = 3;

	public float Width;

	private float value = 0.5f;

	private SpriteRenderer[] Segments;

	public int NumSegments => numberGreen + numberRed + numberYellow;

	public void Awake()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Segments = new SpriteRenderer[NumSegments];
		float num = Width / (float)(NumSegments - 1);
		float num2 = Width / -2f;
		for (int i = 0; i < Segments.Length; i++)
		{
			SpriteRenderer val = (Segments[i] = Object.Instantiate<SpriteRenderer>(SegmentPrefab, ((Component)this).transform));
			((Component)val).transform.localPosition = new Vector3(num2 + (float)i * num, 0f, 0.0001f);
			if (i < numberGreen)
			{
				val.sprite = greenImage;
			}
			else if (i < numberGreen + numberYellow)
			{
				val.sprite = yellowImage;
			}
			else
			{
				val.sprite = redImage;
			}
		}
	}

	public void SetValue(float value)
	{
		this.value = value;
		Update();
	}

	public void Update()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Segments.Length; i++)
		{
			SpriteRenderer val = Segments[i];
			if ((float)i < value * (float)NumSegments)
			{
				if (i < numberGreen)
				{
					val.sprite = greenImage;
				}
				else if (i < numberGreen + numberYellow)
				{
					val.sprite = yellowImage;
				}
				else
				{
					val.sprite = redImage;
				}
				val.color = Color.white;
			}
			else
			{
				if (i < numberGreen)
				{
					val.sprite = greenEmptyImage;
				}
				else if (i < numberGreen + numberYellow)
				{
					val.sprite = yellowEmptyImage;
				}
				else
				{
					val.sprite = redEmptyImage;
				}
				val.color = Color.gray;
			}
		}
	}
}
