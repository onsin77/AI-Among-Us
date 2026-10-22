using UnityEngine;

public class ParallaxController : MonoBehaviour
{
	public ParallaxChild[] Children;

	public float Scale = 1f;

	public void Start()
	{
		Children = ((Component)this).GetComponentsInChildren<ParallaxChild>();
	}

	public void SetParallax(float x)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Children.Length; i++)
		{
			ParallaxChild obj = Children[i];
			Vector3 basePosition = obj.BasePosition;
			_ = Scale;
			if (basePosition.z >= 0f)
			{
				basePosition.x += x / (basePosition.z * Scale + 1f);
			}
			else
			{
				basePosition.x += x * ((0f - basePosition.z) * Scale + 1f);
			}
			((Component)obj).transform.localPosition = basePosition;
		}
	}
}
