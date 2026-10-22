using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeconControl : MonoBehaviour, IUsable
{
	public DeconSystem System;

	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public AudioClip UseSound;

	public ButtonClickedEvent OnUse;

	private const float CooldownDuration = 6f;

	private float cooldown;

	public ImageNames UseIcon => ImageNames.UseButton;

	public float UsableDistance => usableDistance;

	public float PercentCool => cooldown / 6f;

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

	public void Update()
	{
		cooldown = Mathf.Max(cooldown - Time.deltaTime, 0f);
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (System.CurState != DeconSystem.States.Idle)
		{
			canUse = false;
			couldUse = false;
			return 0f;
		}
		float num = float.MaxValue;
		PlayerControl playerControl = pc.Object;
		Vector2 truePosition = playerControl.GetTruePosition();
		Vector3 position = ((Component)this).transform.position;
		position.y -= 0.1f;
		couldUse = playerControl.CanMove && !pc.IsDead && !PhysicsHelpers.AnythingBetween(truePosition, Vector2.op_Implicit(position), Constants.ShipAndObjectsMask, useTriggers: false);
		canUse = couldUse && cooldown == 0f;
		if (canUse)
		{
			num = Vector2.Distance(truePosition, Vector2.op_Implicit(position));
			canUse &= num <= UsableDistance;
		}
		return num;
	}

	public void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			cooldown = 6f;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(UseSound, loop: false);
			}
			((UnityEvent)OnUse).Invoke();
		}
	}
}
