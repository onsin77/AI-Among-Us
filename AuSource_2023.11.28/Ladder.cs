using UnityEngine;

public class Ladder : MonoBehaviour, IUsableCoolDown, IUsable
{
	public byte Id;

	public SpriteRenderer SpotArea;

	public bool IsTop;

	public Ladder Destination;

	public AudioClip UseSound;

	public SpriteRenderer Image;

	public float UsableDistance => 0.5f;

	public float PercentCool => CoolDown / MaxCoolDown;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float CoolDown { get; set; }

	public float MaxCoolDown => 3f;

	public bool IsCoolingDown()
	{
		return CoolDown > 0f;
	}

	private void Update()
	{
		CoolDown = Mathf.Max(CoolDown - Time.deltaTime, 0f);
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		couldUse = !pc.IsDead && playerControl.CanMove;
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
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)Image).material.SetFloat("_Outline", (float)(on ? 1 : 0));
		((Renderer)Image).material.SetColor("_OutlineColor", Color.white);
		((Renderer)Image).material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
	}

	public void Use()
	{
		if (!IsCoolingDown())
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			CanUse(data, out var canUse, out var _);
			if (canUse)
			{
				PlayerControl.LocalPlayer.MyPhysics.RpcClimbLadder(this);
				CoolDown = MaxCoolDown;
			}
		}
	}

	public void SetDestinationCooldown()
	{
		Destination.CoolDown = MaxCoolDown;
	}
}
