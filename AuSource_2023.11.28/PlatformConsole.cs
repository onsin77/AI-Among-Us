using UnityEngine;

public class PlatformConsole : MonoBehaviour, IUsable
{
	public float usableDistance = 0.5f;

	public SpriteRenderer Image;

	public MovingPlatformBehaviour Platform;

	public float UsableDistance => usableDistance;

	public float PercentCool => 0f;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = !pc.IsDead && playerControl.CanMove && !Platform.InUse && Vector2.Distance(Vector2.op_Implicit(((Component)Platform).transform.position), Vector2.op_Implicit(((Component)this).transform.position)) < 2f;
		canUse = couldUse;
		if (canUse)
		{
			Vector2 truePosition = playerControl.GetTruePosition();
			Vector3 position = ((Component)this).transform.position;
			num = Vector2.Distance(truePosition, Vector2.op_Implicit(position));
			canUse &= num <= UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, Vector2.op_Implicit(position), Constants.ShipOnlyMask, useTriggers: false);
		}
		return num;
	}

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

	public void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			Platform.Use();
		}
	}
}
