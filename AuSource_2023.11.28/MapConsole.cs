using UnityEngine;

public class MapConsole : MonoBehaviour, IUsable
{
	public ImageNames useIcon = ImageNames.AdminMapButton;

	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public ImageNames UseIcon => useIcon;

	public float UsableDistance => usableDistance;

	public float PercentCool => 0f;

	public void SetOutline(bool on, bool mainTarget)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Image))
		{
			((Renderer)Image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
			((Renderer)Image).material.SetColor("_OutlineColor", Color.white);
			((Renderer)Image).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = pc.Object.CanMove;
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(playerControl.GetTruePosition(), Vector2.op_Implicit(((Component)this).transform.position));
			canUse &= num <= UsableDistance;
		}
		return num;
	}

	public void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl.LocalPlayer.NetTransform.Halt();
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.CountOverlay
			});
			if (PlayerControl.LocalPlayer.AmOwner)
			{
				((Behaviour)PlayerControl.LocalPlayer.MyPhysics.inputHandler).enabled = true;
				ConsoleJoystick.SetMode_Task();
			}
		}
	}
}
