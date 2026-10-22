using UnityEngine;

public class OneWayShadows : MonoBehaviour
{
	public Collider2D RoomCollider;

	public bool IgnoreImpostor;

	public void OnEnable()
	{
		LightSource.OneWayShadows.Add(((Component)this).gameObject, this);
	}

	public void OnDisable()
	{
		LightSource.OneWayShadows.Remove(((Component)this).gameObject);
	}

	public bool IsIgnored(LightSource lightSource)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!IgnoreImpostor || !PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			return RoomCollider.OverlapPoint(Vector2.op_Implicit(((Component)lightSource).transform.position));
		}
		return true;
	}
}
