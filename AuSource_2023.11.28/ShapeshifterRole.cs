using System.Text;
using UnityEngine;

public class ShapeshifterRole : ImpostorRole
{
	public ShapeshifterEvidence EvidencePrefab;

	public Vector3 EvidenceOffset;

	public ShapeshifterMinigame ShapeshifterMenu;

	private float cooldownSecondsRemaining;

	private float durationSecondsRemaining;

	private bool IsCoolingDown => cooldownSecondsRemaining > 0f;

	public override void UseAbility()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!Player.Data.IsDead && Player.moveable && !Object.op_Implicit((Object)(object)Minigame.Instance) && !IsCoolingDown && !((Object)(object)Player.currentRoleAnimations.Find((RoleEffectAnimation r) => r.effectType == RoleEffectAnimation.EffectType.Shapeshift) != (Object)null) && !Player.IsMushroomMixupActive())
		{
			Player.NetTransform.Halt();
			if (Player.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
			{
				RevertPlayerToNormal();
				return;
			}
			ShapeshifterMinigame shapeshifterMinigame = Object.Instantiate<ShapeshifterMinigame>(ShapeshifterMenu);
			((Component)shapeshifterMinigame).transform.SetParent(((Component)Camera.main).transform, false);
			((Component)shapeshifterMinigame).transform.localPosition = new Vector3(0f, 0f, -50f);
			shapeshifterMinigame.Begin(null);
			durationSecondsRemaining = GameManager.Instance.LogicOptions.GetShapeshifterDuration();
		}
	}

	public void RevertPlayerToNormal(bool shouldAnimate = true, bool setCooldown = true)
	{
		if (Player.CurrentOutfitType == PlayerOutfitType.Shapeshifted || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			Player.CmdCheckRevertShapeshift(shouldAnimate);
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(Ability);
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
			if (setCooldown)
			{
				SetCooldown();
			}
		}
	}

	public override void OnVotingComplete()
	{
		base.OnVotingComplete();
		if (Player.AmOwner)
		{
			RevertPlayerToNormal(shouldAnimate: false, setCooldown: false);
			float shapeshifterDuration = GameManager.Instance.LogicOptions.GetShapeshifterDuration();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFillUp(shapeshifterDuration, shapeshifterDuration);
			cooldownSecondsRemaining = 0f;
			float shapeshifterCooldown = GameManager.Instance.LogicOptions.GetShapeshifterCooldown();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, shapeshifterCooldown);
		}
	}

	public override void SetCooldown()
	{
		cooldownSecondsRemaining = GameManager.Instance.LogicOptions.GetShapeshifterCooldown();
		DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, cooldownSecondsRemaining);
	}

	private bool ShouldUpdateUnshiftTimer()
	{
		if (!Object.op_Implicit((Object)(object)Player) || !Player.AmOwner || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			return false;
		}
		if (Player.inMovingPlat || Player.onLadder)
		{
			return false;
		}
		return true;
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)Player) || !Player.AmOwner || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			return;
		}
		if (!Player.moveable || Player.Data.IsDead || Player.IsMushroomMixupActive())
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
		}
		if (Player.IsMushroomMixupActive())
		{
			cooldownSecondsRemaining = 0f;
			float shapeshifterCooldown = GameManager.Instance.LogicOptions.GetShapeshifterCooldown();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, shapeshifterCooldown);
			return;
		}
		if (cooldownSecondsRemaining > 0f)
		{
			cooldownSecondsRemaining -= Time.deltaTime;
			float shapeshifterCooldown2 = GameManager.Instance.LogicOptions.GetShapeshifterCooldown();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, shapeshifterCooldown2);
		}
		if (Player.CurrentOutfitType == PlayerOutfitType.Shapeshifted && durationSecondsRemaining > 0f && ShouldUpdateUnshiftTimer())
		{
			durationSecondsRemaining -= Time.deltaTime;
			float shapeshifterDuration = GameManager.Instance.LogicOptions.GetShapeshifterDuration();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFillUp(durationSecondsRemaining, shapeshifterDuration);
			if (durationSecondsRemaining <= 0f)
			{
				Player.NetTransform.Halt();
				RevertPlayerToNormal(!Player.inVent);
				DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(Ability);
				DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
			}
		}
	}

	public void SetEvidence()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.Instance.LogicOptions.GetShapeshifterLeaveSkin())
		{
			ShapeshifterEvidence shapeshifterEvidence = Object.Instantiate<ShapeshifterEvidence>(EvidencePrefab);
			Vector3 val = ((Component)Player).transform.position + EvidenceOffset * 0.7f;
			val.z = val.y / 1000f;
			((Component)shapeshifterEvidence).transform.position = val;
		}
	}

	public override void AppendTaskHint(StringBuilder taskStringBuilder)
	{
		RoleBehaviour.AppendTaskHint(this, taskStringBuilder);
	}
}
