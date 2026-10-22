using TMPro;
using UnityEngine;

public class AbilityButton : ActionButton
{
	[SerializeField]
	private GameObject commsDown;

	public override void DoClick()
	{
		if (((Behaviour)this).isActiveAndEnabled && !DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			RoleBehaviour role = PlayerControl.LocalPlayer.Data.Role;
			if (Object.op_Implicit((Object)(object)role.Ability))
			{
				logger.Debug($"Attempting to use role ability for role {role.Role}");
				role.UseAbility();
			}
		}
	}

	public void Refresh(AbilityButtonSettings ability)
	{
		if ((Object)(object)ability != (Object)null)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void SetFromSettings(AbilityButtonSettings settings)
	{
		SetInfiniteUses();
		graphic.sprite = settings.Image;
		graphic.SetCooldownNormalizedUvs();
		((TMP_Text)buttonLabelText).fontSharedMaterial = settings.FontMaterial;
		((TMP_Text)buttonLabelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(settings.Text);
	}

	private void Update()
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsAffectedByComms)
		{
			commsDown.SetActive(true);
		}
		else
		{
			commsDown.SetActive(false);
		}
	}
}
