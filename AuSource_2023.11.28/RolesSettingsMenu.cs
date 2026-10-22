using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

public class RolesSettingsMenu : MonoBehaviour
{
	public List<RoleOptionSetting> AllRoleSettings = new List<RoleOptionSetting>();

	public Transform ItemParent;

	public List<AdvancedRoleSettingsButton> AllAdvancedSettingTabs;

	private OptionBehaviour[] Children;

	public RoleOptionSetting SettingPrefab;

	public GameObject AdvancedRolesSettings;

	public GameObject RoleChancesSettings;

	public float YStart;

	public float YOffset;

	private IRoleOptionsCollection cachedData;

	private void Start()
	{
		Children = ((Component)this).GetComponentsInChildren<OptionBehaviour>(true);
		cachedData = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
		for (int i = 0; i < Children.Length; i++)
		{
			OptionBehaviour optionBehaviour = Children[i];
			optionBehaviour.OnValueChanged = ValueChanged;
			if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && !AmongUsClient.Instance.AmHost)
			{
				optionBehaviour.SetAsPlayer();
			}
		}
	}

	private void Update()
	{
		if (cachedData != GameOptionsManager.Instance.CurrentGameOptions.RoleOptions)
		{
			cachedData = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
			RefreshChildren();
		}
	}

	private void OnEnable()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < AllRoleSettings.Count; i++)
		{
			RoleOptionSetting roleOptionSetting = AllRoleSettings[i];
			if (((Component)roleOptionSetting).gameObject.activeSelf)
			{
				roleOptionSetting.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
				roleOptionSetting.OnValueChanged = ValueChanged;
				Vector3 localPosition = ((Component)roleOptionSetting).transform.localPosition;
				localPosition.y = YStart - (float)i * YOffset;
				((Component)roleOptionSetting).transform.localPosition = localPosition;
			}
		}
		RefreshChildren();
	}

	private void RefreshChildren()
	{
		if (Children == null || Children.Length == 0)
		{
			return;
		}
		for (int i = 0; i < Children.Length; i++)
		{
			OptionBehaviour optionBehaviour = Children[i];
			((Behaviour)optionBehaviour).enabled = false;
			((Behaviour)optionBehaviour).enabled = true;
			IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
			switch (optionBehaviour.Title)
			{
			case StringNames.ScientistCooldown:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.ScientistCooldown);
				break;
			case StringNames.ScientistBatteryCharge:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.ScientistBatteryCharge);
				break;
			case StringNames.ShapeshifterCooldown:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.ShapeshifterCooldown);
				break;
			case StringNames.ShapeshifterDuration:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.ShapeshifterDuration);
				break;
			case StringNames.ShapeshifterLeaveSkin:
				((Renderer)(optionBehaviour as ToggleOption).CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.ShapeshifterLeaveSkin);
				break;
			case StringNames.EngineerCooldown:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.EngineerCooldown);
				break;
			case StringNames.EngineerInVentCooldown:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.EngineerInVentMaxTime);
				break;
			case StringNames.GuardianAngelCooldown:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.GuardianAngelCooldown);
				break;
			case StringNames.GuardianAngelDuration:
				(optionBehaviour as NumberOption).Value = currentGameOptions.GetFloat(FloatOptionNames.ProtectionDurationSeconds);
				break;
			case StringNames.GuardianAngelImpostorSeeProtect:
				((Renderer)(optionBehaviour as ToggleOption).CheckMark).enabled = currentGameOptions.GetBool(BoolOptionNames.ImpostorsCanSeeProtect);
				break;
			}
		}
	}

	private void ValueChanged(OptionBehaviour obj)
	{
		if (obj is RoleOptionSetting)
		{
			RoleOptionSetting roleOptionSetting = obj as RoleOptionSetting;
			GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(roleOptionSetting.Role.Role, roleOptionSetting.RoleMaxCount, roleOptionSetting.RoleChance);
			roleOptionSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
			GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.IsDefaults, value: false);
		}
		else
		{
			IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
			switch (obj.Title)
			{
			case StringNames.ScientistBatteryCharge:
				currentGameOptions.SetFloat(FloatOptionNames.ScientistBatteryCharge, obj.GetFloat());
				break;
			case StringNames.ScientistCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.ScientistCooldown, obj.GetFloat());
				break;
			case StringNames.ShapeshifterCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, obj.GetFloat());
				break;
			case StringNames.ShapeshifterDuration:
				currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, obj.GetFloat());
				break;
			case StringNames.ShapeshifterLeaveSkin:
				currentGameOptions.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, obj.GetBool());
				break;
			case StringNames.EngineerCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.EngineerCooldown, obj.GetFloat());
				break;
			case StringNames.EngineerInVentCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, obj.GetFloat());
				break;
			case StringNames.GuardianAngelCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.GuardianAngelCooldown, obj.GetFloat());
				break;
			case StringNames.GuardianAngelDuration:
				currentGameOptions.SetFloat(FloatOptionNames.ProtectionDurationSeconds, obj.GetFloat());
				break;
			case StringNames.GuardianAngelImpostorSeeProtect:
				currentGameOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, obj.GetBool());
				break;
			default:
				Debug.Log((object)("Ono, unrecognized setting: " + obj.Title));
				break;
			}
			GameOptionsManager.Instance.CurrentGameOptions.SetBool(BoolOptionNames.IsDefaults, value: false);
			RefreshChildren();
		}
		GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		GameManager.Instance.LogicOptions.SyncOptions();
	}

	public void ShowAdvancedSettings(RoleBehaviour role)
	{
		AllAdvancedSettingTabs.ForEach((AdvancedRoleSettingsButton t) =>
		{
			t.Tab.SetActive(t.Type == role.Role);
		});
		RoleChancesSettings.SetActive(false);
		AdvancedRolesSettings.SetActive(true);
		RefreshChildren();
	}

	public void CloseAdvancedSettings()
	{
		RoleChancesSettings.SetActive(true);
		AdvancedRolesSettings.SetActive(false);
	}
}
