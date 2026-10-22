using UnityEngine;

public class FungleSunsetParallax : MonoBehaviour
{
	[SerializeField]
	private Transform content;

	[SerializeField]
	private Transform parallaxRoot;

	[SerializeField]
	private Vector2 parallaxMultiplier;

	private Transform camera;

	private Vector3 origin;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		origin = content.position;
	}

	private void LateUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)camera == (Object)null)
		{
			if ((Object)(object)Camera.main != (Object)null)
			{
				camera = ((Component)Camera.main).transform;
			}
			if ((Object)(object)camera == (Object)null)
			{
				return;
			}
		}
		Vector2 a = Vector2.op_Implicit(parallaxRoot.position - camera.position);
		content.position = origin - Vector2.op_Implicit(a.Mul(parallaxMultiplier));
	}
}
