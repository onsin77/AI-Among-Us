using UnityEngine;

public class MeshAnimator : MonoBehaviour
{
	private MeshFilter filter;

	public Mesh[] Frames;

	public float frameRate;

	private float timer;

	private int frameId;

	private Vector3 originalPosition;

	private Vector3 originalScale;

	public MeshSequence sequence;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		originalPosition = ((Component)this).transform.localPosition;
		originalScale = ((Component)this).transform.localScale;
	}

	private void Start()
	{
		filter = ((Component)this).GetComponent<MeshFilter>();
	}

	private void Update()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		if (!(timer > 1f / frameRate))
		{
			return;
		}
		timer = 0f;
		frameId++;
		if ((Object)(object)sequence == (Object)null)
		{
			if (frameId >= Frames.Length)
			{
				frameId = 0;
			}
			filter.mesh = Frames[frameId];
			return;
		}
		if (frameId >= sequence.Frames.Length)
		{
			frameId = 0;
		}
		float num = 0.01f;
		Vector3 val = sequence.Positions[frameId];
		float num2 = val.x * num;
		float num3 = val.y * num;
		Vector3 val2 = new Vector3(num2, num3, 0f);
		filter.mesh = sequence.Frames[frameId];
		((Component)this).transform.localPosition = val2 + originalPosition;
		((Component)this).transform.localScale = sequence.Scales[frameId].Mul(originalScale);
	}
}
