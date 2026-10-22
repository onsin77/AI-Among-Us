using System.Collections;
using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;

public class TutorialManager : DestroyableSingleton<TutorialManager>
{
	public PlayerControl PlayerPrefab;

	public override void Awake()
	{
		base.Awake();
		StatsManager.Instance = new TutorialStatsManager();
		((MonoBehaviour)this).StartCoroutine(RunTutorial());
		GameDebugCommands.AddCommands();
		TutorialDebugCommands.AddCommands(((Component)this).gameObject);
	}

	public override void OnDestroy()
	{
		StatsManager.Instance = new StatsManager();
		GameDebugCommands.RemoveCommands();
		TutorialDebugCommands.RemoveCommands(((Component)this).gameObject);
		base.OnDestroy();
	}

	private IEnumerator RunTutorial()
	{
		while (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			yield return null;
		}
		ShipStatus.Instance.Timer = 15f;
		while (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			yield return null;
		}
		while (!Object.op_Implicit((Object)(object)GameManager.Instance))
		{
			yield return null;
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			DestroyableSingleton<DiscordManager>.Instance.SetHowToPlay();
		}
		NormalGameOptionsV07 normalGameOptionsV = new NormalGameOptionsV07((ILogger)(object)new UnityLogger());
		normalGameOptionsV.SetInt(Int32OptionNames.NumImpostors, 0);
		normalGameOptionsV.DiscussionTime = 0;
		normalGameOptionsV.NumEmergencyMeetings = 9;
		normalGameOptionsV.GhostsDoTasks = true;
		GameOptionsManager.Instance.CurrentGameOptions = normalGameOptionsV;
		PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
		PlayerControl.LocalPlayer.AdjustLighting();
		PlayerControl.LocalPlayer.cosmetics.SetAsLocalPlayer();
		for (int i = 0; i < ShipStatus.Instance.DummyLocations.Length; i++)
		{
			PlayerControl playerControl = Object.Instantiate<PlayerControl>(PlayerPrefab);
			playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
			GameData.PlayerInfo playerInfo = GameData.Instance.AddPlayer(playerControl);
			AmongUsClient.Instance.Spawn(playerControl);
			playerInfo.DefaultOutfit.dontCensorName = true;
			playerControl.isDummy = true;
			((Component)playerControl).transform.position = ShipStatus.Instance.DummyLocations[i].position;
			((Behaviour)((Component)playerControl).GetComponent<DummyBehaviour>()).enabled = true;
			((Behaviour)playerControl.NetTransform).enabled = false;
			playerControl.SetName(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Dummy) + " " + (i + 1), dontCensor: true);
			byte b = (byte)((i < DataManager.Player.Customization.Color) ? i : (i + 1));
			playerControl.SetColor(b);
			playerControl.SetHat("", b);
			playerControl.SetSkin("", b);
			playerControl.SetPet("");
			playerControl.SetVisor("", b);
			playerControl.SetNamePlate("");
			playerControl.SetLevel(0u);
			GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
		}
		yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
		ShipStatus.Instance.Begin();
		GameManager.Instance.StartGame();
		ShipStatus.Instance.StartSFX();
		Logger.GlobalInstance.Info($"Started Freeplay Game in {(MapNames)AmongUsClient.Instance.TutorialMapId}");
	}
}
