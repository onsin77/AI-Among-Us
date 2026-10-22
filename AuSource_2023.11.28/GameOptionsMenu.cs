using AmongUs.GameOptions;
using UnityEngine;

public class GameOptionsMenu : MonoBehaviour
{
	private IGameOptions cachedData;

	public GameObject ResetButton;

	private OptionBehaviour[] Children;

	private void Start()
	{
		Children = ((Component)this).GetComponentsInChildren<OptionBehaviour>();
		cachedData = GameOptionsManager.Instance.CurrentGameOptions;
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
		if (cachedData != GameOptionsManager.Instance.CurrentGameOptions)
		{
			cachedData = GameOptionsManager.Instance.CurrentGameOptions;
			RefreshChildren();
		}
	}

	private void RefreshChildren()
	{
		for (int i = 0; i < Children.Length; i++)
		{
			OptionBehaviour obj = Children[i];
			((Behaviour)obj).enabled = false;
			((Behaviour)obj).enabled = true;
		}
	}

	private void ValueChanged(OptionBehaviour option)
	{
		if (!Object.op_Implicit((Object)(object)AmongUsClient.Instance) || !AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (option.Title == StringNames.GameRecommendedSettings)
		{
			if (cachedData.GetBool(BoolOptionNames.IsDefaults))
			{
				cachedData.SetBool(BoolOptionNames.IsDefaults, value: false);
			}
			else
			{
				cachedData.SetRecommendations(GameData.Instance.PlayerCount, AmongUsClient.Instance.NetworkMode);
			}
			RefreshChildren();
		}
		else
		{
			IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
			switch (option.Title)
			{
			case StringNames.GameMapName:
				currentGameOptions.SetByte(ByteOptionNames.MapId, (byte)option.GetInt());
				break;
			case StringNames.GamePlayerSpeed:
				currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, option.GetFloat());
				break;
			case StringNames.GameCrewLight:
				currentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, option.GetFloat());
				break;
			case StringNames.GameImpostorLight:
				currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, option.GetFloat());
				break;
			case StringNames.GameKillCooldown:
				currentGameOptions.SetFloat(FloatOptionNames.KillCooldown, option.GetFloat());
				break;
			case StringNames.GameKillDistance:
				currentGameOptions.SetInt(Int32OptionNames.KillDistance, option.GetInt());
				break;
			case StringNames.GameCommonTasks:
				currentGameOptions.SetInt(Int32OptionNames.NumCommonTasks, option.GetInt());
				break;
			case StringNames.GameLongTasks:
				currentGameOptions.SetInt(Int32OptionNames.NumLongTasks, option.GetInt());
				break;
			case StringNames.GameShortTasks:
				currentGameOptions.SetInt(Int32OptionNames.NumShortTasks, option.GetInt());
				break;
			case StringNames.GameNumImpostors:
				currentGameOptions.SetInt(Int32OptionNames.NumImpostors, option.GetInt());
				break;
			case StringNames.GameNumMeetings:
				currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, option.GetInt());
				break;
			case StringNames.GameEmergencyCooldown:
				currentGameOptions.SetInt(Int32OptionNames.EmergencyCooldown, option.GetInt());
				break;
			case StringNames.GameDiscussTime:
				currentGameOptions.SetInt(Int32OptionNames.DiscussionTime, option.GetInt());
				break;
			case StringNames.GameVotingTime:
				currentGameOptions.SetInt(Int32OptionNames.VotingTime, option.GetInt());
				break;
			case StringNames.GameConfirmImpostor:
				currentGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, option.GetBool());
				break;
			case StringNames.GameVisualTasks:
				currentGameOptions.SetBool(BoolOptionNames.VisualTasks, option.GetBool());
				break;
			case StringNames.GameAnonymousVotes:
				currentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, option.GetBool());
				break;
			case StringNames.GameTaskBarMode:
				currentGameOptions.SetInt(Int32OptionNames.TaskBarMode, option.GetInt());
				break;
			case StringNames.UseFlashlight:
				currentGameOptions.SetBool(BoolOptionNames.UseFlashlight, option.GetBool());
				break;
			case StringNames.MinCrewmatesForVitals:
				currentGameOptions.SetInt(Int32OptionNames.CrewmatesRemainingForVitals, option.GetInt());
				break;
			case StringNames.MaxVentUses:
				currentGameOptions.SetInt(Int32OptionNames.CrewmateVentUses, option.GetInt());
				break;
			case StringNames.MaxTimeInVent:
				currentGameOptions.SetFloat(FloatOptionNames.CrewmateTimeInVent, option.GetFloat());
				break;
			case StringNames.EscapeTime:
				currentGameOptions.SetFloat(FloatOptionNames.EscapeTime, option.GetFloat());
				break;
			case StringNames.FinalEscapeTime:
				currentGameOptions.SetFloat(FloatOptionNames.FinalEscapeTime, option.GetFloat());
				break;
			case StringNames.CrewmateFlashlightSize:
				currentGameOptions.SetFloat(FloatOptionNames.CrewmateFlashlightSize, option.GetFloat());
				break;
			case StringNames.ImpostorFlashlightSize:
				currentGameOptions.SetFloat(FloatOptionNames.ImpostorFlashlightSize, option.GetFloat());
				break;
			case StringNames.Impostor:
				currentGameOptions.SetInt(Int32OptionNames.ImpostorPlayerID, option.GetInt());
				break;
			case StringNames.SeekerFinalSpeed:
				currentGameOptions.SetFloat(FloatOptionNames.SeekerFinalSpeed, option.GetFloat());
				break;
			case StringNames.SeekerFinalVents:
				currentGameOptions.SetBool(BoolOptionNames.SeekerFinalVents, option.GetBool());
				break;
			case StringNames.SeekerFinalMap:
				currentGameOptions.SetBool(BoolOptionNames.SeekerFinalMap, option.GetBool());
				break;
			case StringNames.SeekerPings:
				currentGameOptions.SetBool(BoolOptionNames.SeekerPings, option.GetBool());
				break;
			case StringNames.MaxPingTime:
				currentGameOptions.SetFloat(FloatOptionNames.MaxPingTime, option.GetFloat());
				break;
			case StringNames.ShowCrewmateNames:
				currentGameOptions.SetBool(BoolOptionNames.ShowCrewmateNames, option.GetBool());
				break;
			default:
				Debug.Log((object)("Ono, unrecognized setting: " + option.Title));
				break;
			}
			if (currentGameOptions.GetBool(BoolOptionNames.IsDefaults) && option.Title != StringNames.GameMapName)
			{
				currentGameOptions.SetBool(BoolOptionNames.IsDefaults, value: false);
				RefreshChildren();
			}
		}
		GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
		GameManager.Instance.LogicOptions.SyncOptions();
	}
}
