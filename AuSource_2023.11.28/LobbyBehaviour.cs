using System.Collections;
using AmongUsServer;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using TMPro;
using UnityEngine;

[SkipRename]
public class LobbyBehaviour : InnerNetObject
{
	public static LobbyBehaviour Instance;

	public AudioClip SpawnSound;

	public AnimationClip SpawnInClip;

	public Vector2[] SpawnPositions;

	public AudioClip DropShipSound;

	public SkeldShipRoom[] AllRooms;

	private readonly Logger logger = new Logger("LobbyBehaviour", Logger.Level.Warning, Logger.Category.Network);

	private float lastFriendsCheckTime;

	private float optionsTimer;

	private int currentExtensionId = -1;

	public void Start()
	{
		Instance = this;
		((MonoBehaviour)this).StartCoroutine(DelayPlayDropshipAmbience());
		Camera main = Camera.main;
		if (Object.op_Implicit((Object)(object)main))
		{
			FollowerCamera component = ((Component)main).GetComponent<FollowerCamera>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.shakeAmount = 0.03f;
				component.shakePeriod = 400f;
			}
		}
		LobbyDebugCommands.AddCommands();
	}

	private void Update()
	{
		HandleFriendsDataCaching();
	}

	public void RpcExtendLobbyTimer()
	{
		if (AmongUsClient.Instance.AmHost && !DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 61, (SendOption)1);
			val.WritePacked(currentExtensionId);
			AmongUsClient.Instance.FinishRpcImmediately(val);
		}
	}

	private IEnumerator DelayPlayDropshipAmbience()
	{
		SoundManager.Instance.StopAllSound();
		yield return (object)new WaitForSeconds(0.5f);
		AudioSource val = SoundManager.Instance.PlayNamedSound("DropShipAmb", DropShipSound, loop: true, SoundManager.Instance.AmbienceChannel);
		val.loop = true;
		val.pitch = 1.2f;
	}

	public void FixedUpdate()
	{
		optionsTimer += Time.fixedDeltaTime;
		if (!(optionsTimer < 0.25f))
		{
			optionsTimer = 0f;
			if (GameOptionsManager.Instance.CurrentGameOptions != null)
			{
				int numPlayers = (Object.op_Implicit((Object)(object)GameData.Instance) ? GameData.Instance.PlayerCount : 10);
				((TMP_Text)DestroyableSingleton<HudManager>.Instance.GameSettings).text = GameOptionsManager.Instance.CurrentGameOptions.ToHudString(numPlayers);
				((Component)DestroyableSingleton<HudManager>.Instance.GameSettings).gameObject.SetActive(true);
			}
		}
	}

	public override void OnDestroy()
	{
		SoundManager.Instance.StopNamedSound("DropShipAmb");
		LobbyDebugCommands.RemoveCommands();
		base.OnDestroy();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.LobbyTimeExpiring:
		{
			int timeRemainingSeconds = reader.ReadPackedInt32();
			bool flag2 = reader.ReadBoolean();
			int hostId = 255;
			int extensionId = 0;
			int extendedTimeSeconds = 0;
			if (flag2)
			{
				hostId = reader.ReadPackedInt32();
				extensionId = reader.ReadPackedInt32();
				extendedTimeSeconds = reader.ReadPackedInt32();
			}
			HandleLobbyTimerExtensionRequest(timeRemainingSeconds, flag2, hostId, extensionId, extendedTimeSeconds);
			break;
		}
		case RpcCalls.ExtendLobbyTimer:
		{
			int num = reader.ReadPackedInt32();
			bool flag = reader.ReadBoolean();
			ExtensionFailureReasons extensionFailureReasons = ExtensionFailureReasons.Invalid;
			if (!flag)
			{
				extensionFailureReasons = (ExtensionFailureReasons)reader.ReadByte();
				logger.Warning($"Lobby extension {num} failed due to reason {extensionFailureReasons}!");
			}
			else
			{
				LobbyTimerExtended();
			}
			break;
		}
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	private void HandleFriendsDataCaching()
	{
		if (!((Object)(object)GameData.Instance == (Object)null) && GameData.Instance.AllPlayers != null && GameData.Instance.AllPlayers.Count > 1 && DestroyableSingleton<FriendsListManager>.InstanceExists && Time.time > lastFriendsCheckTime + 4f)
		{
			DestroyableSingleton<FriendsListManager>.Instance.UpdateFriendsDataWithPlayers(GameData.Instance.AllPlayers);
			lastFriendsCheckTime += Time.time;
		}
	}

	private void HandleLobbyTimerExtensionRequest(int timeRemainingSeconds, bool isExtensionAvailable, int hostId, int extensionId, int extendedTimeSeconds)
	{
		DestroyableSingleton<HudManager>.Instance.ShowLobbyTimer(timeRemainingSeconds);
	}

	private void LobbyTimerExtended()
	{
		DestroyableSingleton<HudManager>.Instance.OnLobbyTimerExtended();
	}
}
