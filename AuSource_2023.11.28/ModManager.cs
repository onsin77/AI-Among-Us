using Beebyte.Obfuscator;
using UnityEngine;

[Skip]
public class ModManager : DestroyableSingleton<ModManager>
{
	public SpriteRenderer ModStamp;

	public Camera localCamera;

	public void ShowModStamp()
	{
		((Renderer)ModStamp).enabled = true;
	}

	private void LateUpdate()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (!((Renderer)ModStamp).enabled)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)localCamera))
		{
			if (DestroyableSingleton<HudManager>.InstanceExists)
			{
				localCamera = ((Component)DestroyableSingleton<HudManager>.Instance).GetComponentInChildren<Camera>();
			}
			else
			{
				localCamera = Camera.main;
			}
		}
		((Component)ModStamp).transform.position = AspectPosition.ComputeWorldPosition(localCamera, AspectPosition.EdgeAlignments.RightTop, new Vector3(0.6f, 0.6f, localCamera.nearClipPlane + 0.1f));
	}
}
