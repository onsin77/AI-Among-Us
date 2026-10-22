using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class NumberOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public float Value = 1f;

	private float oldValue = float.MaxValue;

	public float Increment;

	public FloatRange ValidRange = new FloatRange(0f, 2f);

	public string FormatString = "0.0";

	public bool ZeroIsInfinity;

	public NumberSuffixes SuffixType = NumberSuffixes.Multiplier;

	public void OnEnable()
	{
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(Title);
		FixedUpdate();
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		switch (Title)
		{
		case StringNames.GamePlayerSpeed:
			Value = currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);
			break;
		case StringNames.GameCrewLight:
			Value = currentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
			break;
		case StringNames.GameImpostorLight:
			Value = currentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);
			break;
		case StringNames.GameKillCooldown:
			Value = currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
			break;
		case StringNames.GameCommonTasks:
			Value = currentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
			break;
		case StringNames.GameLongTasks:
			Value = currentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
			break;
		case StringNames.GameShortTasks:
			Value = currentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
			break;
		case StringNames.GameNumImpostors:
			Value = currentGameOptions.GetInt(Int32OptionNames.NumImpostors);
			break;
		case StringNames.GameNumMeetings:
			Value = currentGameOptions.GetInt(Int32OptionNames.NumEmergencyMeetings);
			break;
		case StringNames.GameEmergencyCooldown:
			Value = currentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown);
			break;
		case StringNames.GameDiscussTime:
			Value = currentGameOptions.GetInt(Int32OptionNames.DiscussionTime);
			break;
		case StringNames.GameVotingTime:
			Value = currentGameOptions.GetInt(Int32OptionNames.VotingTime);
			break;
		case StringNames.MinCrewmatesForVitals:
			Value = currentGameOptions.GetInt(Int32OptionNames.CrewmatesRemainingForVitals);
			break;
		case StringNames.MaxVentUses:
			Value = currentGameOptions.GetInt(Int32OptionNames.CrewmateVentUses);
			break;
		case StringNames.MaxTimeInVent:
			Value = currentGameOptions.GetFloat(FloatOptionNames.CrewmateTimeInVent);
			break;
		case StringNames.EscapeTime:
			Value = currentGameOptions.GetFloat(FloatOptionNames.EscapeTime);
			break;
		case StringNames.FinalEscapeTime:
			Value = currentGameOptions.GetFloat(FloatOptionNames.FinalEscapeTime);
			break;
		case StringNames.CrewmateFlashlightSize:
			Value = currentGameOptions.GetFloat(FloatOptionNames.CrewmateFlashlightSize);
			break;
		case StringNames.ImpostorFlashlightSize:
			Value = currentGameOptions.GetFloat(FloatOptionNames.ImpostorFlashlightSize);
			break;
		case StringNames.SeekerFinalSpeed:
			Value = currentGameOptions.GetFloat(FloatOptionNames.SeekerFinalSpeed);
			break;
		case StringNames.MaxPingTime:
			Value = currentGameOptions.GetFloat(FloatOptionNames.MaxPingTime);
			break;
		}
	}

	private void FixedUpdate()
	{
		if (oldValue != Value)
		{
			oldValue = Value;
			if (ZeroIsInfinity && Mathf.Abs(Value) < 0.0001f)
			{
				((TMP_Text)ValueText).text = "∞";
				return;
			}
			if (SuffixType == NumberSuffixes.None)
			{
				((TMP_Text)ValueText).text = Value.ToString(FormatString);
				return;
			}
			if (SuffixType == NumberSuffixes.Multiplier)
			{
				((TMP_Text)ValueText).text = Value.ToString(FormatString) + "x";
				return;
			}
			((TMP_Text)ValueText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, Value.ToString(FormatString));
		}
	}

	public void Increase()
	{
		Value = ValidRange.Clamp(Value + Increment);
		OnValueChanged(this);
	}

	public void Decrease()
	{
		Value = ValidRange.Clamp(Value - Increment);
		OnValueChanged(this);
	}

	public override float GetFloat()
	{
		return Value;
	}

	public override int GetInt()
	{
		return (int)Value;
	}
}
