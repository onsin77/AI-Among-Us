using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class RoleOptionSetting : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro CountText;

	public TextMeshPro ChanceText;

	public RoleBehaviour Role;

	public int RoleMaxCount;

	public int RoleChance;

	private int RoleMax => Mathf.Min(Role.MaxCount, 15);

	public void SetRole(IRoleOptionsCollection options)
	{
		((TMP_Text)TitleText).text = Role.NiceName;
		UpdateValuesAndText(options);
	}

	public void UpdateValuesAndText(IRoleOptionsCollection options)
	{
		RoleMaxCount = options.GetNumPerGame(Role.Role);
		RoleChance = options.GetChancePerGame(Role.Role);
		((TMP_Text)CountText).text = RoleMaxCount.ToString();
		((TMP_Text)ChanceText).text = RoleChance + "%";
	}

	public void IncreaseCount()
	{
		if (RoleMaxCount == 0)
		{
			RoleChance = 50;
		}
		RoleMaxCount = Mathf.Clamp(RoleMaxCount + 1, 0, RoleMax);
		OnValueChanged(this);
		ShowRoleDetails();
	}

	public void DecreaseCount()
	{
		RoleMaxCount = Mathf.Clamp(RoleMaxCount - 1, 0, RoleMax);
		if (RoleMaxCount == 0)
		{
			RoleChance = 0;
		}
		OnValueChanged(this);
		ShowRoleDetails();
	}

	public void IncreaseChance()
	{
		if (RoleChance == 0)
		{
			RoleMaxCount = 1;
		}
		RoleChance = Mathf.Clamp(RoleChance + 10, 10, 100);
		OnValueChanged(this);
		ShowRoleDetails();
	}

	public void DecreaseChance()
	{
		RoleChance = Mathf.Clamp(RoleChance - 10, 0, 100);
		if (RoleChance == 0)
		{
			RoleMaxCount = 0;
		}
		OnValueChanged(this);
		ShowRoleDetails();
	}

	public void ShowRoleDetails()
	{
		((TMP_Text)GameSettingMenu.Instance.RoleName).text = Role.NiceName;
		((TMP_Text)GameSettingMenu.Instance.RoleBlurb).text = Role.BlurbLong;
		GameSettingMenu.Instance.RoleIcon.sprite = Role.Ability.Image;
	}

	public void ShowAdvancedOptions()
	{
		ShowRoleDetails();
		GameSettingMenu.Instance.ShowAdvancedRoleOptions(Role);
	}
}
