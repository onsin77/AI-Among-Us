using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UseButton : ActionButton
{
	public UseButtonSettings[] UseSettings;

	private Dictionary<ImageNames, UseButtonSettings> fastUseSettings;

	private IUsable currentTarget;

	public void Awake()
	{
		fastUseSettings = UseSettings.ToDictionary((UseButtonSettings t) => t.ButtonType);
	}

	private void FixedUpdate()
	{
		if (currentTarget is IUsableCoolDown usableCoolDown)
		{
			SetCoolDown(usableCoolDown.CoolDown, usableCoolDown.MaxCoolDown);
		}
	}

	public void SetTarget(IUsable target)
	{
		if (fastUseSettings == null)
		{
			return;
		}
		currentTarget = target;
		if (target != null)
		{
			ImageNames key = currentTarget.UseIcon;
			if (!fastUseSettings.ContainsKey(key))
			{
				key = ImageNames.UseButton;
			}
			SetFromSettings(fastUseSettings[key]);
			SetEnabled();
			if (currentTarget is IUsableCoolDown usableCoolDown)
			{
				SetCoolDown(usableCoolDown.CoolDown, usableCoolDown.MaxCoolDown);
			}
			else
			{
				ResetCoolDown();
			}
			SetCooldownFill(target.PercentCool);
		}
		else
		{
			ResetCoolDown();
			SetFromSettings(fastUseSettings[ImageNames.UseButton]);
			SetDisabled();
		}
	}

	public override void DoClick()
	{
		if (((Behaviour)this).isActiveAndEnabled && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && !isCoolingDown)
		{
			PlayerControl.LocalPlayer.UseClosest();
		}
	}

	public void Refresh()
	{
		SetTarget(currentTarget);
	}

	private void SetFromSettings(UseButtonSettings settings)
	{
		graphic.sprite = settings.Image;
		graphic.SetCooldownNormalizedUvs();
		((TMP_Text)buttonLabelText).fontSharedMaterial = settings.FontMaterial;
		((TMP_Text)buttonLabelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(settings.Text);
	}
}
