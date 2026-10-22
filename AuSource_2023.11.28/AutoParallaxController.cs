using UnityEngine;

public class AutoParallaxController : MonoBehaviour
{
	public ParallaxChild[] Children;

	public float XScale = 1f;

	public float YScale = 1f;

	public void Start()
	{
		Children = ((Component)this).GetComponentsInChildren<ParallaxChild>();
	}

	public void Update()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		for (int i = 0; i < Children.Length; i++)
		{
			ParallaxChild obj = Children[i];
			Vector3 basePosition = obj.BasePosition;
			if (basePosition.z < 0f)
			{
				basePosition.z = 0f - basePosition.z;
			}
			basePosition.x -= truePosition.x / (basePosition.z * XScale);
			basePosition.y -= truePosition.y / (basePosition.z * YScale);
			((Component)obj).transform.localPosition = basePosition;
		}
	}
}
