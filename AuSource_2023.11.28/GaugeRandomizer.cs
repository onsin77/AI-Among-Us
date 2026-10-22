using UnityEngine;

public class GaugeRandomizer : MonoBehaviour
{
	public FloatRange Range;

	public SpriteRenderer Gauge;

	public float Frequency = 1f;

	public float Offset = 1f;

	private float naturalY;

	private float naturalSizeY;

	private Color goodLineColor = new Color(100f, 193f, 255f);

	public void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		naturalSizeY = Gauge.size.y;
		naturalY = ((Component)this).transform.localPosition.y;
	}

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		float num = Range.Lerp(Mathf.PerlinNoise(Offset, Time.time * Frequency) / 2f + 0.5f);
		Vector2 size = Gauge.size;
		size.y = num;
		Gauge.size = size;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y = naturalY - (naturalSizeY - num) / 2f;
		((Component)this).transform.localPosition = localPosition;
	}

	public GaugeRandomizer()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
	}
}
