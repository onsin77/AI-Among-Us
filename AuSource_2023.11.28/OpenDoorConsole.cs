using UnityEngine;

public class OpenDoorConsole : MonoBehaviour, IUsable
{
	private ImageNames useIcon = ImageNames.UseButton;

	public float usableDisance = 1.5f;

	private OpenableDoor myDoor;

	private SpriteRenderer image;

	public ImageNames UseIcon => useIcon;

	public float UsableDistance => usableDisance;

	public float PercentCool => 0f;

	public void Awake()
	{
		myDoor = ((Component)this).GetComponent<OpenableDoor>();
		image = ((Component)this).GetComponent<SpriteRenderer>();
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(pc.Object.GetTruePosition(), Vector2.op_Implicit(((Component)this).transform.position));
		couldUse = !pc.IsDead && !myDoor.IsOpen;
		canUse = couldUse && num <= UsableDistance;
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)image))
		{
			((Renderer)image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
			((Renderer)image).material.SetColor("_OutlineColor", Color.white);
			((Renderer)image).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(myDoor.Id | 0x40));
			myDoor.SetDoorway(open: true);
		}
	}
}
