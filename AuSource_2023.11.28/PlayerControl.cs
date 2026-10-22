using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using AmongUs.GameOptions;
using AmongUs.QuickChat;
using Assets.CoreScripts;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using Innersloth.Assets;
using PowerTools;
using UnityEngine;

[SkipRename]
public class PlayerControl : InnerNetObject
{
	public class ColliderComparer : IEqualityComparer<Collider2D>
	{
		public static readonly ColliderComparer Instance = new ColliderComparer();

		public bool Equals(Collider2D x, Collider2D y)
		{
			return (Object)(object)x == (Object)(object)y;
		}

		public int GetHashCode(Collider2D obj)
		{
			return ((Object)obj).GetInstanceID();
		}
	}

	public class UsableComparer : IEqualityComparer<IUsable>
	{
		public static readonly UsableComparer Instance = new UsableComparer();

		public bool Equals(IUsable x, IUsable y)
		{
			return x == y;
		}

		public int GetHashCode(IUsable obj)
		{
			return obj.GetHashCode();
		}
	}

	private int LastStartCounter;

	public byte PlayerId = byte.MaxValue;

	public string FriendCode;

	public string Puid;

	public float MaxReportDistance = 5f;

	public bool moveable = true;

	public CosmeticsLayer cosmetics;

	[HideInInspector]
	public bool ForceKillTimerContinue;

	public bool inVent;

	public bool walkingToVent;

	public bool petting;

	public bool inMovingPlat;

	public bool onLadder;

	public bool protectedByGuardianThisRound;

	public bool shapeshifting;

	public bool waitingForShapeshiftResponse;

	public bool isKilling;

	public static PlayerControl LocalPlayer;

	private GameData.PlayerInfo _cachedData;

	private int protectedByGuardianId = -1;

	private float flashlightAngle;

	private int shapeshiftTargetPlayerId = -1;

	public AudioSource FootSteps;

	public AudioClip KillSfx;

	public KillAnimation[] KillAnimations;

	[SerializeField]
	private float killTimer;

	public int RemainingEmergencies;

	public LightSource LightPrefab;

	private LightSource lightSource;

	[HideInInspector]
	public Collider2D Collider;

	[HideInInspector]
	public PlayerPhysics MyPhysics;

	[HideInInspector]
	public CustomNetworkTransform NetTransform;

	[SerializeField]
	private Collider2D clickKillCollider;

	public Vector3 defaultCosmeticsScale = new Vector3(0.5f, 0.5f, 1f);

	public List<PlayerTask> myTasks = new List<PlayerTask>();

	public List<RoleEffectAnimation> currentRoleAnimations;

	public GameObject TargetFlashlight;

	public bool isDummy;

	public bool notRealPlayer;

	private readonly Logger logger = new Logger("PlayerControl", Logger.Level.Debug, Logger.Category.Gameplay);

	private readonly List<IPlayerVisibleItem> visibilityItems = new List<IPlayerVisibleItem>();

	private Collider2D[] hitBuffer = new Collider2D[60];

	private IUsable closest;

	private bool isNew = true;

	private Rigidbody2D rigidbody2D;

	public static List<PlayerControl> AllPlayerControls = new List<PlayerControl>();

	private Dictionary<Collider2D, IUsable[]> cache = new Dictionary<Collider2D, IUsable[]>(ColliderComparer.Instance);

	private List<IUsable> itemsInRange = new List<IUsable>();

	private List<IUsable> newItemsInRange = new List<IUsable>();

	private byte scannerCount;

	private bool roleAssigned;

	public bool CanMove
	{
		get
		{
			if (moveable && !inVent && !MyPhysics.DoingCustomAnimation && !shapeshifting && !waitingForShapeshiftResponse && !Object.op_Implicit((Object)(object)Minigame.Instance) && (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen && !DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)) && (!Object.op_Implicit((Object)(object)ControllerManager.Instance) || !ControllerManager.Instance.IsUiControllerActive) && (!Object.op_Implicit((Object)(object)MapBehaviour.Instance) || !MapBehaviour.Instance.IsOpenStopped) && !Object.op_Implicit((Object)(object)MeetingHud.Instance) && !Object.op_Implicit((Object)(object)PlayerCustomizationMenu.Instance) && !Object.op_Implicit((Object)(object)ExileController.Instance))
			{
				return !Object.op_Implicit((Object)(object)IntroCutscene.Instance);
			}
			return false;
		}
	}

	public bool IsKillTimerEnabled
	{
		get
		{
			if ((moveable || petting) && !inVent && !shapeshifting && (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)) && !Object.op_Implicit((Object)(object)MeetingHud.Instance) && !Object.op_Implicit((Object)(object)PlayerCustomizationMenu.Instance) && !Object.op_Implicit((Object)(object)ExileController.Instance))
			{
				return !Object.op_Implicit((Object)(object)IntroCutscene.Instance);
			}
			return false;
		}
	}

	public GameData.PlayerOutfit CurrentOutfit
	{
		get
		{
			if (Data == null)
			{
				logger.Warning("Tried to access CurrentOutfit before Data was set");
				return new GameData.PlayerOutfit();
			}
			return Data.Outfits[CurrentOutfitType];
		}
	}

	public PlayerOutfitType CurrentOutfitType { get; private set; }

	public float FlashlightAngle
	{
		get
		{
			return flashlightAngle;
		}
		set
		{
			if (value != flashlightAngle)
			{
				flashlightAngle = value;
			}
		}
	}

	public GameData.PlayerInfo Data
	{
		get
		{
			if (_cachedData == null)
			{
				if (!Object.op_Implicit((Object)(object)GameData.Instance))
				{
					return null;
				}
				_cachedData = GameData.Instance.GetPlayerById(PlayerId);
			}
			return _cachedData;
		}
	}

	public bool Visible
	{
		get
		{
			return cosmetics.Visible;
		}
		set
		{
			cosmetics.Visible = value;
			visibilityItems.ForEach((IPlayerVisibleItem v) =>
			{
				v.Visible = value;
			});
		}
	}

	public PlayerBodyTypes BodyType => GameManager.Instance.GetBodyType(this);

	public void RpcSetScanner(bool value)
	{
		byte b = ++scannerCount;
		if (AmongUsClient.Instance.AmClient)
		{
			SetScanner(value, b);
		}
		if (GameManager.Instance.LogicOptions.GetVisualTasks())
		{
			MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 15, (SendOption)1);
			val.Write(value);
			val.Write(b);
			val.EndMessage();
		}
	}

	public void RpcUsePlatform()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
			if (Object.op_Implicit((Object)(object)airshipStatus))
			{
				airshipStatus.GapPlatform.Use(this);
			}
		}
		else
		{
			AmongUsClient.Instance.StartRpc(NetId, 32, (SendOption)1).EndMessage();
		}
	}

	public void CmdCheckUseZipline(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
	{
		if (AmongUsClient.Instance.AmLocalHost || AmongUsClient.Instance.AmModdedHost)
		{
			CheckUseZipline(target, ziplineBehaviour, fromTop);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 51, (SendOption)1, AmongUsClient.Instance.HostId);
		val.Write(fromTop);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void CmdCheckSporeTrigger(Mushroom mushroom)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckSporeTrigger(mushroom);
			return;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 54, (SendOption)1, AmongUsClient.Instance.HostId);
		val.WritePacked(mushroom.Id);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcPlayAnimation(byte animType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			PlayAnimation(animType);
		}
		if (GameManager.Instance.LogicOptions.GetVisualTasks())
		{
			MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 0, (SendOption)0);
			val.Write(animType);
			val.EndMessage();
		}
	}

	public void RpcSetStartCounter(int secondsLeft)
	{
		int num = LastStartCounter++;
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 18, (SendOption)1);
		val.WritePacked(num);
		val.Write((sbyte)secondsLeft);
		val.EndMessage();
	}

	public void RpcCompleteTask(uint idx)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			CompleteTask(idx);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 1, (SendOption)1);
		val.WritePacked(idx);
		val.EndMessage();
	}

	public void RpcSetRole(RoleTypes roleType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetRole(roleType);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 44, (SendOption)1);
		val.Write((ushort)roleType);
		val.EndMessage();
	}

	public void CmdCheckName(string name)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckName(name);
			return;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 5, (SendOption)1, AmongUsClient.Instance.HostId);
		val.Write(name);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcSetLevel(uint level)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetLevel(level);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 38, (SendOption)1);
		val.WritePacked(level);
		val.EndMessage();
	}

	public void RpcSetVisor(string visorId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			int valueOrDefault = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetVisor(visorId, valueOrDefault);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 42, (SendOption)1);
		val.Write(visorId);
		val.EndMessage();
	}

	public void RpcSetNamePlate(string namePlateId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetNamePlate(namePlateId);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 43, (SendOption)1);
		val.Write(namePlateId);
		val.EndMessage();
	}

	public void RpcSetSkin(string skinId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			int valueOrDefault = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetSkin(skinId, valueOrDefault);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 40, (SendOption)1);
		val.Write(skinId);
		val.EndMessage();
	}

	public void RpcSetHat(string hatId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			int valueOrDefault = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetHat(hatId, valueOrDefault);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 39, (SendOption)1);
		val.Write(hatId);
		val.EndMessage();
	}

	public void RpcSetPet(string petId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetPet(petId);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 41, (SendOption)1);
		val.Write(petId);
		val.EndMessage();
	}

	public void RpcSetName(string name)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetName(name);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 6, (SendOption)1);
		val.Write(name);
		val.EndMessage();
	}

	public void CmdCheckColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckColor(bodyColor);
			return;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 7, (SendOption)1, AmongUsClient.Instance.HostId);
		val.Write(bodyColor);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcSetColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetColor(bodyColor);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 8, (SendOption)1);
		val.Write(bodyColor);
		val.EndMessage();
	}

	public bool RpcSendChat(string chatText)
	{
		chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
		if (string.IsNullOrWhiteSpace(chatText))
		{
			return false;
		}
		if (AmongUsClient.Instance.AmClient && Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
		{
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText);
		}
		if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 13, (SendOption)1);
		val.Write(chatText);
		val.EndMessage();
		return true;
	}

	public bool RpcSendQuickChat(QuickChatPhraseBuilderResult data)
	{
		string text = data.ToChatText();
		if (string.IsNullOrWhiteSpace(text) || data == null || !data.IsValid())
		{
			return false;
		}
		if (AmongUsClient.Instance.AmClient && Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
		{
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, text, censor: false);
		}
		if (text.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 33, (SendOption)1);
		QuickChatNetData.Serialize(data, val);
		val.EndMessage();
		return true;
	}

	public void RpcSendChatNote(byte srcPlayerId, ChatNoteTypes noteType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(srcPlayerId);
			DestroyableSingleton<HudManager>.Instance.Chat.AddChatNote(playerById, noteType);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 16, (SendOption)1);
		val.Write(srcPlayerId);
		val.Write((byte)noteType);
		val.EndMessage();
	}

	public void CmdReportDeadBody(GameData.PlayerInfo target)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			ReportDeadBody(target);
			return;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 11, (SendOption)1);
		val.Write(target?.PlayerId ?? byte.MaxValue);
		val.EndMessage();
	}

	public void RpcStartMeeting(GameData.PlayerInfo info)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			StartMeeting(info);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 14, (SendOption)1);
		val.Write(info?.PlayerId ?? byte.MaxValue);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void CmdCheckMurder(PlayerControl target)
	{
		isKilling = true;
		if (AmongUsClient.Instance.AmLocalHost || AmongUsClient.Instance.AmModdedHost)
		{
			CheckMurder(target);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 47, (SendOption)1);
		val.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void CmdCheckProtect(PlayerControl target)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckProtect(target);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 48, (SendOption)1);
		val.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcMurderPlayer(PlayerControl target, bool didSucceed)
	{
		MurderResultFlags murderResultFlags = (didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError);
		MurderResultFlags murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;
		if (AmongUsClient.Instance.AmClient)
		{
			MurderPlayer(target, murderResultFlags2);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 12, (SendOption)1);
		val.WriteNetObject(target);
		val.Write((int)murderResultFlags2);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcUseZipline(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			ziplineBehaviour.Use(target, fromTop);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 52, (SendOption)1);
		val.Write(fromTop);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcTriggerSpores(Mushroom mushroom)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			mushroom.TriggerSpores();
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 53, (SendOption)1);
		val.Write(mushroom.Id);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcProtectPlayer(PlayerControl target, int colorId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			ProtectPlayer(target, colorId);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 45, (SendOption)1);
		val.WriteNetObject(target);
		val.Write(colorId);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcShapeshift(PlayerControl target, bool shouldAnimate)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			Shapeshift(target, shouldAnimate);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 46, (SendOption)1);
		val.WriteNetObject(target);
		val.Write(shouldAnimate);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void CmdCheckShapeshift(PlayerControl target, bool shouldAnimate)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckShapeshift(target, shouldAnimate);
			return;
		}
		waitingForShapeshiftResponse = true;
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 55, (SendOption)1, AmongUsClient.Instance.HostId);
		val.WriteNetObject(target);
		val.Write(shouldAnimate);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	public void RpcRejectShapeshift()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			MessageWriter msg = AmongUsClient.Instance.StartRpcImmediately(NetId, 56, (SendOption)1);
			AmongUsClient.Instance.FinishRpcImmediately(msg);
		}
	}

	public void RpcSyncSettings(byte[] optionsByteArray)
	{
		if (AmongUsClient.Instance.AmHost && !DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 2, (SendOption)1);
			val.WriteBytesAndSize(optionsByteArray);
			AmongUsClient.Instance.FinishRpcImmediately(val);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(isNew);
		}
		writer.Write(PlayerId);
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			isNew = reader.ReadBoolean();
		}
		PlayerId = reader.ReadByte();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.UsePlatform:
			if (AmongUsClient.Instance.AmHost)
			{
				AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
				if (Object.op_Implicit((Object)(object)airshipStatus))
				{
					airshipStatus.GapPlatform.Use(this);
					SetDirtyBit(4096u);
				}
			}
			break;
		case RpcCalls.UseZipline:
		{
			FungleShipStatus fungleShipStatus = ShipStatus.Instance as FungleShipStatus;
			if (Object.op_Implicit((Object)(object)fungleShipStatus))
			{
				bool fromTop = reader.ReadBoolean();
				fungleShipStatus.Zipline.Use(this, fromTop);
			}
			break;
		}
		case RpcCalls.CheckZipline:
		{
			FungleShipStatus fungleShipStatus2 = ShipStatus.Instance as FungleShipStatus;
			if (Object.op_Implicit((Object)(object)fungleShipStatus2))
			{
				bool fromTop2 = reader.ReadBoolean();
				CheckUseZipline(this, fungleShipStatus2.Zipline, fromTop2);
			}
			break;
		}
		case RpcCalls.CheckSpore:
		{
			int mushroomId2 = reader.ReadPackedInt32();
			FungleShipStatus fungleShipStatus4 = ShipStatus.Instance as FungleShipStatus;
			if (Object.op_Implicit((Object)(object)fungleShipStatus4))
			{
				Mushroom mushroomFromId = fungleShipStatus4.GetMushroomFromId(mushroomId2);
				CheckSporeTrigger(mushroomFromId);
			}
			break;
		}
		case RpcCalls.TriggerSpores:
		{
			int mushroomId = reader.ReadPackedInt32();
			FungleShipStatus fungleShipStatus3 = ShipStatus.Instance as FungleShipStatus;
			if (Object.op_Implicit((Object)(object)fungleShipStatus3))
			{
				fungleShipStatus3.TriggerSporesFromMushroom(mushroomId);
			}
			break;
		}
		case RpcCalls.PlayAnimation:
			PlayAnimation(reader.ReadByte());
			break;
		case RpcCalls.Exiled:
			Exiled();
			break;
		case RpcCalls.CheckName:
			CheckName(reader.ReadString());
			break;
		case RpcCalls.SetName:
			SetName(reader.ReadString());
			break;
		case RpcCalls.CheckColor:
			CheckColor(reader.ReadByte());
			break;
		case RpcCalls.SetColor:
			SetColor(reader.ReadByte());
			break;
		case RpcCalls.SetSkinStr:
		{
			int valueOrDefault = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetSkin(reader.ReadString(), valueOrDefault);
			break;
		}
		case RpcCalls.SetHatStr:
		{
			int valueOrDefault3 = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetHat(reader.ReadString(), valueOrDefault3);
			break;
		}
		case RpcCalls.SetPetStr:
			SetPet(reader.ReadString());
			break;
		case RpcCalls.SetVisorStr:
		{
			int valueOrDefault2 = (Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
			SetVisor(reader.ReadString(), valueOrDefault2);
			break;
		}
		case RpcCalls.SetNamePlateStr:
			SetNamePlate(reader.ReadString());
			break;
		case RpcCalls.SetLevel:
			SetLevel(reader.ReadPackedUInt32());
			break;
		case RpcCalls.SetRole:
			SetRole((RoleTypes)reader.ReadUInt16());
			break;
		case RpcCalls.ReportDeadBody:
		{
			GameData.PlayerInfo playerById3 = GameData.Instance.GetPlayerById(reader.ReadByte());
			ReportDeadBody(playerById3);
			break;
		}
		case RpcCalls.StartMeeting:
		{
			GameData.PlayerInfo playerById2 = GameData.Instance.GetPlayerById(reader.ReadByte());
			StartMeeting(playerById2);
			break;
		}
		case RpcCalls.CheckMurder:
		{
			PlayerControl target5 = reader.ReadNetObject<PlayerControl>();
			CheckMurder(target5);
			break;
		}
		case RpcCalls.CheckProtect:
		{
			PlayerControl target4 = reader.ReadNetObject<PlayerControl>();
			CheckProtect(target4);
			break;
		}
		case RpcCalls.CheckShapeshift:
		{
			PlayerControl target3 = reader.ReadNetObject<PlayerControl>();
			CheckShapeshift(target3, reader.ReadBoolean());
			break;
		}
		case RpcCalls.RejectShapeshift:
			RejectShapeshift();
			break;
		case RpcCalls.MurderPlayer:
		{
			PlayerControl target2 = reader.ReadNetObject<PlayerControl>();
			MurderResultFlags resultFlags = (MurderResultFlags)reader.ReadInt32();
			MurderPlayer(target2, resultFlags);
			break;
		}
		case RpcCalls.ProtectPlayer:
		{
			PlayerControl target = reader.ReadNetObject<PlayerControl>();
			ProtectPlayer(target, reader.ReadByte());
			break;
		}
		case RpcCalls.SetStartCounter:
		{
			int num = reader.ReadPackedInt32();
			sbyte startCounter = reader.ReadSByte();
			if (DestroyableSingleton<GameStartManager>.InstanceExists && LastStartCounter < num)
			{
				LastStartCounter = num;
				DestroyableSingleton<GameStartManager>.Instance.SetStartCounter(startCounter);
			}
			break;
		}
		case RpcCalls.CompleteTask:
			CompleteTask(reader.ReadPackedUInt32());
			break;
		case RpcCalls.SendChat:
		{
			string chatText = reader.ReadString();
			if (Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
			{
				DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText);
			}
			break;
		}
		case RpcCalls.SendQuickChat:
		{
			QuickChatPhraseBuilderResult quickChatPhraseBuilderResult = QuickChatNetData.Deserialize(reader);
			if (Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
			{
				DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, quickChatPhraseBuilderResult.ToChatText(), censor: false);
			}
			break;
		}
		case RpcCalls.SendChatNote:
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
			DestroyableSingleton<HudManager>.Instance.Chat.AddChatNote(playerById, (ChatNoteTypes)reader.ReadByte());
			break;
		}
		case RpcCalls.SetScanner:
			SetScanner(reader.ReadBoolean(), reader.ReadByte());
			break;
		case RpcCalls.Shapeshift:
		{
			PlayerControl targetPlayer = reader.ReadNetObject<PlayerControl>();
			Shapeshift(targetPlayer, reader.ReadBoolean());
			break;
		}
		case RpcCalls.SyncSettings:
		case RpcCalls.SetInfected:
		case RpcCalls.SetHat:
		case RpcCalls.SetSkin:
		case RpcCalls.SetPet:
		case RpcCalls.EnterVent:
		case RpcCalls.ExitVent:
		case RpcCalls.SnapTo:
		case RpcCalls.CloseMeeting:
		case RpcCalls.VotingComplete:
		case RpcCalls.CastVote:
		case RpcCalls.ClearVote:
		case RpcCalls.AddVote:
		case RpcCalls.CloseDoorsOfType:
		case (RpcCalls)28:
		case RpcCalls.SetTasks:
		case (RpcCalls)30:
		case RpcCalls.ClimbLadder:
		case RpcCalls.BootFromVent:
		case RpcCalls.UpdateSystem:
		case RpcCalls.SetVisor:
		case RpcCalls.SetNamePlate:
		case RpcCalls.Pet:
		case RpcCalls.CancelPet:
			break;
		}
	}

	public void SetKillTimer(float time)
	{
		if (Data.Role.CanUseKillButton)
		{
			float num = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
			if (!(num <= 0f))
			{
				killTimer = Mathf.Clamp(time, 0f, num);
				DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(killTimer, num);
			}
		}
	}

	private void Awake()
	{
		MyPhysics = ((Component)this).GetComponent<PlayerPhysics>();
		NetTransform = ((Component)this).GetComponent<CustomNetworkTransform>();
		Collider = ((Component)this).GetComponent<Collider2D>();
		rigidbody2D = ((Component)this).GetComponent<Rigidbody2D>();
		currentRoleAnimations = new List<RoleEffectAnimation>();
		if (!notRealPlayer)
		{
			AllPlayerControls.Add(this);
		}
	}

	private void OnEnable()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged += SetColorBlindTag;
		DataManager.Settings.Input.OnInputModeChanged += AdjustLighting;
		AdjustLighting();
	}

	private void OnDisable()
	{
		DataManager.Settings.Accessibility.OnColorBlindModeChanged -= SetColorBlindTag;
		DataManager.Settings.Input.OnInputModeChanged -= AdjustLighting;
	}

	private IEnumerator Start()
	{
		while (PlayerId == byte.MaxValue)
		{
			yield return null;
		}
		while ((Object)(object)GameManager.Instance == (Object)null || (Object)(object)GameData.Instance == (Object)null)
		{
			yield return null;
		}
		RemainingEmergencies = GameManager.Instance.LogicOptions.GetNumEmergencyMeetings();
		SetColorBlindTag();
		cosmetics.UpdateVisibility();
		if (AmOwner)
		{
			lightSource = Object.Instantiate<LightSource>(LightPrefab, ((Component)this).transform, false);
			lightSource.Initialize(Vector2.op_Implicit(Collider.offset * 0.5f));
			LocalPlayer = this;
			cosmetics.SetAsLocalPlayer();
			((Component)Camera.main).GetComponent<FollowerCamera>().SetTarget((MonoBehaviour)(object)this);
			SetName(DataManager.Player.Customization.Name);
			SetColor(DataManager.Player.Customization.Color);
			if (Application.targetFrameRate > 30)
			{
				MyPhysics.EnableInterpolation();
			}
			CmdCheckName(DataManager.Player.Customization.Name);
			CmdCheckColor(DataManager.Player.Customization.Color);
			RpcSetPet(DataManager.Player.Customization.Pet);
			RpcSetHat(DataManager.Player.Customization.Hat);
			RpcSetSkin(DataManager.Player.Customization.Skin);
			RpcSetVisor(DataManager.Player.Customization.Visor);
			RpcSetNamePlate(DataManager.Player.Customization.NamePlate);
			RpcSetLevel(DataManager.Player.Stats.Level);
			yield return null;
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(ClientInitialize());
		}
		MyPhysics.SetBodyType(BodyType);
		if (isNew)
		{
			isNew = false;
			((MonoBehaviour)this).StartCoroutine(MyPhysics.CoSpawnPlayer(LobbyBehaviour.Instance));
		}
		if ((Object)(object)LocalPlayer == (Object)(object)this)
		{
			((Behaviour)clickKillCollider).enabled = false;
		}
	}

	private IEnumerator ClientInitialize()
	{
		Visible = false;
		while (!Object.op_Implicit((Object)(object)GameData.Instance) || Data == null || Data.IsIncomplete)
		{
			yield return null;
		}
		GameData.PlayerOutfit defaultOutfit = Data.DefaultOutfit;
		SetName(defaultOutfit.PlayerName, isDummy);
		SetColor(defaultOutfit.ColorId);
		SetHat(defaultOutfit.HatId, defaultOutfit.ColorId);
		SetSkin(defaultOutfit.SkinId, defaultOutfit.ColorId);
		SetPet(defaultOutfit.PetId);
		SetVisor(defaultOutfit.VisorId, defaultOutfit.ColorId);
		SetNamePlate(defaultOutfit.NamePlateId);
		SetLevel(Data.PlayerLevel);
		Visible = true;
		yield return null;
	}

	public override void OnDestroy()
	{
		if (!notRealPlayer)
		{
			AllPlayerControls.Remove(this);
		}
		base.OnDestroy();
	}

	private void FixedUpdate()
	{
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		GameData.PlayerInfo data = Data;
		if (data == null || (Object)(object)data.Role == (Object)null)
		{
			return;
		}
		if (data.IsDead && Object.op_Implicit((Object)(object)LocalPlayer) && LocalPlayer.Data != null)
		{
			Visible = LocalPlayer.Data.IsDead;
			cosmetics.SetPetVisible(v: true);
		}
		if (!AmOwner)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			float num = ShipStatus.Instance.CalculateLightRadius(data);
			if (!Mathf.Approximately(num, lightSource.ViewDistance))
			{
				AdjustLighting();
			}
			lightSource.SetViewDistance(num);
		}
		PlayerControl playerControl = data.Role.FindClosestTarget();
		bool flag = data.Role.CanUseKillButton && !data.IsDead;
		if ((IsKillTimerEnabled || ForceKillTimerContinue) & flag)
		{
			SetKillTimer(killTimer - Time.fixedDeltaTime);
			DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(playerControl);
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
			Data.Role.SetPlayerTarget(playerControl);
		}
		if (CanMove && ((Behaviour)LocalPlayer.MyPhysics.inputHandler).enabled)
		{
			((Behaviour)LocalPlayer.MyPhysics.inputHandler).enabled = false;
		}
		if (CanMove || inVent)
		{
			newItemsInRange.Clear();
			bool flag2 = (GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !data.IsDead) && (!Object.op_Implicit((Object)(object)AmongUsClient.Instance) || !AmongUsClient.Instance.IsGameOver) && CanMove;
			Vector2 truePosition = GetTruePosition();
			int num2 = Physics2D.OverlapCircleNonAlloc(truePosition, MaxReportDistance, hitBuffer, Constants.Usables);
			IUsable usable = null;
			float num3 = float.MaxValue;
			bool flag3 = false;
			List<Vent> list = new List<Vent>();
			for (int i = 0; i < num2; i++)
			{
				Collider2D val = hitBuffer[i];
				if (!cache.TryGetValue(val, out var value))
				{
					IUsable[] array = (cache[val] = ((Component)val).GetComponents<IUsable>());
					value = array;
				}
				if (value != null && (flag2 || inVent))
				{
					IUsable[] array = value;
					foreach (IUsable usable2 in array)
					{
						float num4 = usable2.CanUse(data, out var canUse, out var couldUse);
						if (canUse | couldUse)
						{
							newItemsInRange.Add(usable2);
						}
						if (canUse && num4 < num3)
						{
							if (usable2 is Vent)
							{
								list.Add(usable2 as Vent);
								continue;
							}
							num3 = num4;
							usable = usable2;
						}
					}
				}
				if (flag2 && !data.IsDead && !flag3 && ((Component)val).tag == "DeadBody")
				{
					DeadBody component = ((Component)val).GetComponent<DeadBody>();
					if (((Behaviour)component).enabled && Vector2.Distance(truePosition, component.TruePosition) <= MaxReportDistance && !PhysicsHelpers.AnythingBetween(truePosition, component.TruePosition, Constants.ShipAndObjectsMask, useTriggers: false))
					{
						flag3 = true;
					}
				}
			}
			Vent vent = ((list.Count > 0) ? list.First() : null);
			for (int num5 = itemsInRange.Count - 1; num5 > -1; num5--)
			{
				IUsable item = itemsInRange[num5];
				int num6 = newItemsInRange.FindIndex((IUsable usable4) => usable4 == item);
				if (num6 == -1)
				{
					item.SetOutline(on: false, mainTarget: false);
					itemsInRange.RemoveAt(num5);
				}
				else
				{
					newItemsInRange.RemoveAt(num6);
					bool flag4 = false;
					flag4 = ((!(item is Vent)) ? (usable == item) : ((Object)(object)(item as Vent) == (Object)(object)vent));
					item.SetOutline(on: true, flag4);
				}
			}
			for (int num7 = 0; num7 < newItemsInRange.Count; num7++)
			{
				IUsable usable3 = newItemsInRange[num7];
				bool flag5 = false;
				flag5 = ((!(usable3 is Vent)) ? (usable == usable3) : ((Object)(object)(usable3 as Vent) == (Object)(object)vent));
				usable3.SetOutline(on: true, flag5);
				itemsInRange.Add(usable3);
			}
			closest = usable;
			DestroyableSingleton<HudManager>.Instance.ToggleUseAndPetButton(usable, flag2, CanPet());
			DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(flag3);
			DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(vent);
			Data.Role.SetUsableTarget(vent);
		}
		else
		{
			closest = null;
			DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
			DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(Vent.currentVent);
			DestroyableSingleton<HudManager>.Instance.PetButton.SetDisabled();
			DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(isActive: false);
			Data.Role.SetUsableTarget(Vent.currentVent);
			if (Object.op_Implicit((Object)(object)PlayerCustomizationMenu.Instance))
			{
				((Component)DestroyableSingleton<HudManager>.Instance.UseButton).gameObject.SetActive(false);
			}
		}
		DestroyableSingleton<HudManager>.Instance.SabotageButton.Refresh();
		DestroyableSingleton<HudManager>.Instance.AdminButton.Refresh();
	}

	public void AnimateCustom(AnimationClip anim)
	{
		MyPhysics.AnimateCustom(anim);
	}

	public void OnGameStart()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.ToggleNameVisible(GameManager.Instance.LogicOptions.GetShowCrewmateNames());
		if ((Object)(object)cosmetics.CurrentPet != (Object)null)
		{
			cosmetics.CurrentPet.SetGettingPet(petting: false, Vector2.op_Implicit(((Component)cosmetics.CurrentPet).transform.position));
		}
	}

	public void OnGameEnd()
	{
		cosmetics.ToggleNameVisible(visible: true);
	}

	public void UseClosest()
	{
		if (closest != null)
		{
			closest.Use();
			closest = null;
			DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
		}
	}

	public void RegisterVisibilityItem(IPlayerVisibleItem obj)
	{
		visibilityItems.Add(obj);
		obj.Visible = Visible;
	}

	public void UnregisterVisibilityItem(IPlayerVisibleItem obj)
	{
		visibilityItems.Remove(obj);
	}

	public void TryPet()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (CanPet())
		{
			Collider2D collider = Collider;
			Bounds bounds = Collider.bounds;
			if (!PhysicsHelpers.AnythingBetween(collider, Vector2.op_Implicit(bounds.center), Vector2.op_Implicit(cosmetics.CurrentPet.PettingPlayerPosition), Constants.ShipAndAllObjectsMask, useTriggers: false))
			{
				MyPhysics.RpcPet(Vector2.op_Implicit(cosmetics.CurrentPet.PettingPlayerPosition), Vector2.op_Implicit(((Component)cosmetics.CurrentPet).transform.position));
			}
		}
		closest = null;
		DestroyableSingleton<HudManager>.Instance.PetButton.SetTarget();
	}

	public void ReportClosest()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (AmongUsClient.Instance.IsGameOver || LocalPlayer.Data.IsDead)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(GetTruePosition(), MaxReportDistance, Constants.PlayersOnlyMask);
		foreach (Collider2D val in array)
		{
			if (((Component)val).tag != "DeadBody")
			{
				continue;
			}
			DeadBody component = ((Component)val).GetComponent<DeadBody>();
			if (Object.op_Implicit((Object)(object)component) && !component.Reported)
			{
				component.OnClick();
				if (component.Reported)
				{
					break;
				}
			}
		}
	}

	public void PlayStepSound()
	{
		if (!Constants.ShouldPlaySfx() || (Object)(object)LocalPlayer != (Object)(object)this)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			for (int i = 0; i < LobbyBehaviour.Instance.AllRooms.Length; i++)
			{
				SoundGroup soundGroup = LobbyBehaviour.Instance.AllRooms[i].MakeFootstep(this);
				if (Object.op_Implicit((Object)(object)soundGroup))
				{
					AudioClip clip = soundGroup.Random();
					FootSteps.clip = clip;
					FootSteps.Play();
					break;
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			return;
		}
		for (int j = 0; j < ShipStatus.Instance.AllStepWatchers.Length; j++)
		{
			SoundGroup soundGroup2 = ShipStatus.Instance.AllStepWatchers[j].MakeFootstep(this);
			if (Object.op_Implicit((Object)(object)soundGroup2))
			{
				AudioClip clip2 = soundGroup2.Random();
				FootSteps.clip = clip2;
				FootSteps.Play();
				break;
			}
		}
	}

	private void SetScanner(bool on, byte cnt)
	{
		if (cnt >= scannerCount)
		{
			scannerCount = cnt;
			MyPhysics.Animations.PlayScanner(on, Data.IsDead, cosmetics.FlipX);
		}
	}

	public Vector2 GetTruePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(((Component)this).transform.position) + Collider.offset;
	}

	public void SetTasks(List<GameData.TaskInfo> tasks)
	{
		((MonoBehaviour)this).StartCoroutine(CoSetTasks(tasks));
	}

	private IEnumerator CoSetTasks(List<GameData.TaskInfo> tasks)
	{
		while (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			yield return null;
		}
		if (AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.TaskStuff.SetActive(true);
			StatsManager.Instance.IncrementStat(StringNames.StatsGamesStarted);
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				DestroyableSingleton<AchievementManager>.Instance.OnMatchStart(Data.Role.Role);
			}
		}
		myTasks.DestroyAll();
		Data.Role.SpawnTaskHeader(this);
		for (int i = 0; i < tasks.Count; i++)
		{
			GameData.TaskInfo taskInfo = tasks[i];
			NormalPlayerTask taskById = ShipStatus.Instance.GetTaskById(taskInfo.TypeId);
			NormalPlayerTask normalPlayerTask = Object.Instantiate<NormalPlayerTask>(taskById, ((Component)this).transform);
			normalPlayerTask.Id = taskInfo.Id;
			normalPlayerTask.Index = taskById.Index;
			normalPlayerTask.Owner = this;
			normalPlayerTask.Initialize();
			logger.Info($"Assigned task {((Object)normalPlayerTask).name} to {PlayerId}");
			myTasks.Add(normalPlayerTask);
		}
	}

	public PlayerTask AddSystemTask(SystemTypes system)
	{
		if (!AmOwner)
		{
			return null;
		}
		PlayerTask playerTask = Object.Instantiate<PlayerTask>(ShipStatus.Instance.GetSabotageTask(system), ((Component)this).transform);
		playerTask.Id = 255u;
		playerTask.Owner = this;
		playerTask.Initialize();
		myTasks.Add(playerTask);
		return playerTask;
	}

	public void RemoveTask(PlayerTask task)
	{
		task.OnRemove();
		myTasks.Remove(task);
		GameData.Instance.TutOnlyRemoveTask(PlayerId, task.Id);
		DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
		Object.Destroy((Object)(object)((Component)task).gameObject);
	}

	public void ClearTasks()
	{
		for (int i = 0; i < myTasks.Count; i++)
		{
			PlayerTask playerTask = myTasks[i];
			playerTask.OnRemove();
			Object.Destroy((Object)(object)((Component)playerTask).gameObject);
		}
		myTasks.Clear();
	}

	public void Die(DeathReason reason, bool assignGhostRole)
	{
		logger.Debug($"Player {PlayerId} dying for reason {reason}");
		if (!DestroyableSingleton<TutorialManager>.InstanceExists && AmOwner)
		{
			StatsManager.Instance.LastGameStarted = DateTime.MinValue;
			StatsManager.Instance.BanPoints--;
		}
		TempData.LastDeathReason = reason;
		if (inMovingPlat && ShipStatus.Instance is FungleShipStatus fungleShipStatus)
		{
			fungleShipStatus.Zipline.CancelZiplineUseForPlayer(this);
		}
		cosmetics.AnimatePetMourning();
		FixMixedUpOutfit();
		Data.IsDead = true;
		((Behaviour)clickKillCollider).enabled = false;
		((Component)this).gameObject.layer = LayerMask.NameToLayer("Ghost");
		cosmetics.SetNameMask(alive: false);
		cosmetics.PettingHand.StopPetting();
		if (walkingToVent)
		{
			inVent = false;
			Vent.currentVent = null;
			moveable = true;
			((MonoBehaviour)MyPhysics).StopAllCoroutines();
		}
		GameManager.Instance.OnPlayerDeath(this, assignGhostRole);
		if (!AmOwner)
		{
			return;
		}
		DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(visible: true);
		((Component)DestroyableSingleton<HudManager>.Instance.ShadowQuad).gameObject.SetActive(false);
		AdjustLighting();
		foreach (PlayerControl allPlayerControl in AllPlayerControls)
		{
			allPlayerControl.cosmetics.ToggleNameVisible(GameManager.Instance.LogicOptions.GetShowCrewmateNames());
		}
	}

	public void Revive()
	{
		Data.IsDead = false;
		((Component)this).gameObject.layer = LayerMask.NameToLayer("Players");
		MyPhysics.ResetMoveState();
		((Behaviour)clickKillCollider).enabled = true;
		cosmetics.SetPetSource(this);
		cosmetics.SetNameMask(alive: true);
		if (AmOwner)
		{
			((Component)DestroyableSingleton<HudManager>.Instance.ShadowQuad).gameObject.SetActive(true);
			DestroyableSingleton<HudManager>.Instance.KillButton.ToggleVisible(Data.Role.IsImpostor);
			DestroyableSingleton<HudManager>.Instance.AdminButton.ToggleVisible(Data.Role.IsImpostor);
			DestroyableSingleton<HudManager>.Instance.SabotageButton.ToggleVisible(Data.Role.IsImpostor);
			DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(Data.Role.IsImpostor);
			DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
			DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(visible: false);
		}
	}

	public void OnClick()
	{
		if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek && LocalPlayer.Data.RoleType == RoleTypes.Impostor && Data.RoleType != RoleTypes.Impostor)
		{
			DestroyableSingleton<HudManager>.Instance.KillButton.CheckClick(this);
		}
	}

	public void CheckSeekerHand_AnimEvent()
	{
		if (cosmetics.HasHat())
		{
			((Component)cosmetics.HatHand).gameObject.SetActive(true);
			cosmetics.ToggleHat(b: true);
		}
	}

	public void PlayAnimation(byte animType)
	{
		switch ((TaskTypes)animType)
		{
		case TaskTypes.ClearAsteroids:
			ShipStatus.Instance.FireWeapon();
			break;
		case TaskTypes.PrimeShields:
			ShipStatus.Instance.StartShields();
			break;
		case TaskTypes.EmptyChute:
		case TaskTypes.EmptyGarbage:
			ShipStatus.Instance.OpenHatch();
			break;
		}
	}

	public void CompleteTask(uint idx)
	{
		PlayerTask playerTask = myTasks.Find((PlayerTask p) => p.Id == idx);
		if (Object.op_Implicit((Object)(object)playerTask))
		{
			if (AmOwner && Data.Role.Role == RoleTypes.Scientist && (Data.Role as ScientistRole).Recharge())
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsScientistChargesGained);
			}
			GameData.Instance.CompleteTask(this, idx);
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.TaskCompleted(Data, playerTask);
			playerTask.Complete();
			logger.Debug($"Player {PlayerId} completed task {playerTask.TaskType}");
		}
		else
		{
			logger.Warning(PlayerId + ": Server didn't have task: " + idx);
		}
		GameManager.Instance.FinishTask(playerTask);
	}

	public bool AllTasksCompleted()
	{
		for (int i = 0; i < myTasks.Count; i++)
		{
			PlayerTask playerTask = myTasks[i];
			if (playerTask is NormalPlayerTask && !playerTask.IsComplete)
			{
				return false;
			}
		}
		return true;
	}

	public bool MustCleanVent(int ventId)
	{
		if (!Data.Role.TasksCountTowardProgress)
		{
			return false;
		}
		NormalPlayerTask normalPlayerTask = myTasks.Find((PlayerTask p) => p.TaskType == TaskTypes.VentCleaning) as NormalPlayerTask;
		if ((Object)(object)normalPlayerTask == (Object)null)
		{
			return false;
		}
		if (!normalPlayerTask.IsComplete)
		{
			return normalPlayerTask.Data[0] == ventId;
		}
		return false;
	}

	private void SetRole(RoleTypes role)
	{
		bool flag = RoleManager.IsGhostRole(role);
		if (!DestroyableSingleton<TutorialManager>.InstanceExists && roleAssigned && !flag)
		{
			return;
		}
		roleAssigned = true;
		if (flag)
		{
			DestroyableSingleton<RoleManager>.Instance.SetRole(this, role);
			Data.Role.SpawnTaskHeader(this);
			if (AmOwner)
			{
				((Component)DestroyableSingleton<HudManager>.Instance.ReportButton).gameObject.SetActive(false);
			}
			return;
		}
		RemainingEmergencies = GameManager.Instance.LogicOptions.GetNumEmergencyMeetings();
		DestroyableSingleton<RoleManager>.Instance.SetRole(this, role);
		Data.Role.SpawnTaskHeader(this);
		MyPhysics.SetBodyType(BodyType);
		if (AmOwner)
		{
			if (Data.Role.IsImpostor)
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsGamesImpostor);
				StatsManager.Instance.ResetStat(StringNames.StatsCrewmateStreak);
			}
			else
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsGamesCrewmate);
				StatsManager.Instance.IncrementStat(StringNames.StatsCrewmateStreak);
			}
			((Component)DestroyableSingleton<HudManager>.Instance.MapButton).gameObject.SetActive(true);
			((Component)DestroyableSingleton<HudManager>.Instance.ReportButton).gameObject.SetActive(true);
			((Component)DestroyableSingleton<HudManager>.Instance.UseButton).gameObject.SetActive(true);
		}
		if (!DestroyableSingleton<TutorialManager>.InstanceExists && AllPlayerControls.All((PlayerControl pc) => pc.roleAssigned || pc.Data.Disconnected))
		{
			AllPlayerControls.ForEach((PlayerControl pc) =>
			{
				PlayerNameColor.Set(pc);
			});
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
			DestroyableSingleton<HudManager>.Instance.HideGameLoader();
		}
	}

	public void Exiled()
	{
		Die(DeathReason.Exile, assignGhostRole: true);
		if (AmOwner)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsTimesEjected);
		}
	}

	public void CheckName(string name)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		if (allPlayers.Any((GameData.PlayerInfo i) => i.PlayerId != PlayerId && i.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase)))
		{
			for (int num = 1; num < 100; num++)
			{
				string text = name + " " + num;
				bool flag = false;
				for (int num2 = 0; num2 < allPlayers.Count; num2++)
				{
					if (allPlayers[num2].PlayerId != PlayerId && allPlayers[num2].PlayerName.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					name = text;
					break;
				}
			}
		}
		RpcSetName(name);
		GameData.Instance.UpdateName(PlayerId, name);
	}

	public void SetKinematic(bool b)
	{
		rigidbody2D.isKinematic = b;
	}

	public void SetName(string name, bool dontCensor = false)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateName(PlayerId, name, dontCensor);
		}
		RawSetName(name);
	}

	public void CheckColor(byte bodyColor)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		int num = 0;
		while (num++ < 100 && (bodyColor >= Palette.PlayerColors.Length || allPlayers.Any((GameData.PlayerInfo p) => !p.Disconnected && p.PlayerId != PlayerId && p.DefaultOutfit.ColorId == bodyColor)))
		{
			bodyColor = (byte)((bodyColor + 1) % Palette.PlayerColors.Length);
		}
		RpcSetColor(bodyColor);
	}

	public void SetHatAndVisorAlpha(float a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		white.a = a;
		cosmetics.SetHatColor(white);
		cosmetics.SetVisorAlpha(a);
	}

	public void SetColor(int bodyColor)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateColor(PlayerId, bodyColor);
		}
		RawSetColor(bodyColor);
	}

	public void SetNamePlate(string namePlateId)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateNamePlate(PlayerId, namePlateId);
		}
	}

	public void SetVisor(string visorId, int colorId)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateVisor(PlayerId, visorId);
		}
		RawSetVisor(visorId, colorId);
	}

	public void SetLevel(uint level)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateLevel(PlayerId, level);
		}
	}

	public void SetSkin(string skinId, int color)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateSkin(PlayerId, skinId);
		}
		RawSetSkin(skinId, color);
	}

	public void SetHat(string hatId, int colorId)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdateHat(PlayerId, hatId);
		}
		RawSetHat(hatId, colorId);
	}

	public void SetPet(string petId)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdatePet(PlayerId, petId);
		}
		if (Data != null && Data.DefaultOutfit != null)
		{
			RawSetPet(petId, Data.DefaultOutfit.ColorId);
		}
	}

	public void SetPet(string petId, int colorId)
	{
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance.UpdatePet(PlayerId, petId);
		}
		RawSetPet(petId, colorId);
	}

	public void SetPetPosition(Vector3 newPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetPetPosition(newPosition);
	}

	public PetBehaviour GetPet()
	{
		return cosmetics.GetPet();
	}

	public static void SetSkinImageAsync(SkinData skin, int colorId, SpriteRenderer target)
	{
		AddressableAssetExtensions.LoadAssetAsync<SkinViewData>(((Component)target).gameObject, (IAddressableAssetProvider<SkinViewData>)skin, (Action<SkinViewData>)((SkinViewData skinView) =>
		{
			if (!((Object)(object)target == (Object)null) && !((Component)target).gameObject.IsDestroyedOrNull())
			{
				target.sprite = skinView.IdleFrame;
				if (skinView.MatchPlayerColor)
				{
					((Renderer)target).sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
					PlayerMaterial.SetColors(colorId, (Renderer)(object)target);
				}
				else
				{
					((Renderer)target).sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
				}
			}
		}), (Action)null);
	}

	private void ReportDeadBody(GameData.PlayerInfo target)
	{
		if (AmongUsClient.Instance.IsGameOver || Object.op_Implicit((Object)(object)MeetingHud.Instance) || (target == null && LocalPlayer.myTasks.Any(PlayerTask.TaskIsEmergency)) || Data.IsDead)
		{
			return;
		}
		MeetingRoomManager.Instance.AssignSelf(this, target);
		if (AmongUsClient.Instance.AmHost && !GameManager.Instance.CheckTaskCompletion())
		{
			if (target == null)
			{
				logger.Debug("Calling emergency meeting");
			}
			else
			{
				logger.Debug($"Reporting dead body {target.PlayerId}");
			}
			DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(this);
			RpcStartMeeting(target);
		}
	}

	private void CheckUseZipline(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		logger.Debug($"Checking if {PlayerId} can use zipline");
		if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)target))
		{
			logger.Warning("Invalid zipline use, player is null");
			return;
		}
		GameData.PlayerInfo data = target.Data;
		if (data == null || data.IsDead || target.inMovingPlat)
		{
			logger.Warning($"Invalid zipline use from {target.PlayerId}");
			return;
		}
		if (Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			logger.Warning("Tried to zipline while a meeting was starting");
			return;
		}
		Vector3 val = ziplineBehaviour.GetHandlePos(fromTop) - ((Component)target).transform.position;
		if (val.magnitude > 3f)
		{
			logger.Info($"{target} was denied the zipline: distance={val.magnitude}");
		}
		else
		{
			RpcUseZipline(target, ziplineBehaviour, fromTop);
		}
	}

	private void CheckSporeTrigger(Mushroom mushroom)
	{
		logger.Debug($"Checking if mushroom id {mushroom.Id} can trigger spores");
		if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
		{
			logger.Warning("Tried trigger mushroom while a game is over or as non-host");
		}
		else if (Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			logger.Warning("Tried trigger mushroom while a meeting was starting");
		}
		else
		{
			RpcTriggerSpores(mushroom);
		}
	}

	public void StartMeeting(GameData.PlayerInfo target)
	{
		bool flag = target == null;
		DestroyableSingleton<UnityTelemetry>.Instance.WriteMeetingStarted(flag);
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MeetingStarted(Data, target != null);
		ShipStatus.Instance.StartMeeting(this, target);
		if (AmOwner)
		{
			if (flag)
			{
				RemainingEmergencies--;
				StatsManager.Instance.IncrementStat(StringNames.StatsEmergenciesCalled);
			}
			else
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsBodiesReported);
			}
		}
	}

	public void ResetForMeeting()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cosmetics.CurrentPet != (Object)null)
		{
			cosmetics.CurrentPet.SetGettingPet(petting: false, Vector2.op_Implicit(((Component)cosmetics.CurrentPet).transform.position));
		}
		if (!((Behaviour)((Component)this).GetComponent<DummyBehaviour>()).enabled)
		{
			MyPhysics.ExitAllVents();
			ShipStatus.Instance.SpawnPlayer(this, GameData.Instance.PlayerCount, initialSpawn: false);
		}
		RemoveProtection();
		((Behaviour)NetTransform).enabled = true;
		MyPhysics.ResetMoveState();
		for (int i = 0; i < currentRoleAnimations.Count; i++)
		{
			if ((Object)(object)currentRoleAnimations[i] != (Object)null && (Object)(object)((Component)currentRoleAnimations[i]).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)currentRoleAnimations[i]).gameObject);
				logger.Error("Encountered a null Role Animation while destroying.");
			}
		}
		inMovingPlat = false;
		isKilling = false;
		currentRoleAnimations.Clear();
	}

	public void CheckMurder(PlayerControl target)
	{
		logger.Debug(string.Format("Checking if {0} murdered {1}", PlayerId, ((Object)(object)target == (Object)null) ? "null player" : target.PlayerId.ToString()));
		isKilling = false;
		if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)target) || Data.IsDead || !Data.Role.IsImpostor || Data.Disconnected)
		{
			int num = (Object.op_Implicit((Object)(object)target) ? target.PlayerId : (-1));
			logger.Warning($"Bad kill from {PlayerId} to {num}");
			RpcMurderPlayer(target, didSucceed: false);
			return;
		}
		GameData.PlayerInfo data = target.Data;
		if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
		{
			logger.Warning("Invalid target data for kill");
			RpcMurderPlayer(target, didSucceed: false);
		}
		else if (Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			logger.Warning("Tried to kill while a meeting was starting");
			RpcMurderPlayer(target, didSucceed: false);
		}
		else
		{
			isKilling = true;
			RpcMurderPlayer(target, didSucceed: true);
		}
	}

	private void CheckShapeshift(PlayerControl target, bool shouldAnimate)
	{
		logger.Debug(string.Format("Checking if {0} can shapeshift into {1}", PlayerId, ((Object)(object)target == (Object)null) ? "null player" : target.PlayerId.ToString()));
		if (!AmongUsClient.Instance.IsGameOver && AmongUsClient.Instance.AmHost)
		{
			if (!Object.op_Implicit((Object)(object)target) || target.Data == null || Data.IsDead || Data.Role.Role != RoleTypes.Shapeshifter || Data.Disconnected)
			{
				int num = (Object.op_Implicit((Object)(object)target) ? target.PlayerId : (-1));
				logger.Warning($"Bad shapeshift from {PlayerId} to {num}");
				RpcRejectShapeshift();
			}
			else if (target.IsMushroomMixupActive() & shouldAnimate)
			{
				logger.Warning("Tried to shapeshift while mushroom mixup was active");
				RpcRejectShapeshift();
			}
			else if (Object.op_Implicit((Object)(object)MeetingHud.Instance) & shouldAnimate)
			{
				logger.Warning("Tried to shapeshift while a meeting was starting");
				RpcRejectShapeshift();
			}
			else
			{
				RpcShapeshift(target, shouldAnimate);
			}
		}
	}

	public bool IsMushroomMixupActive()
	{
		FungleShipStatus fungleShipStatus = ShipStatus.Instance as FungleShipStatus;
		if ((Object)(object)fungleShipStatus != (Object)null && fungleShipStatus.IsMushroomMixupActive())
		{
			return true;
		}
		return CurrentOutfitType == PlayerOutfitType.MushroomMixup;
	}

	public void MurderPlayer(PlayerControl target, MurderResultFlags resultFlags)
	{
		isKilling = false;
		logger.Debug($"{PlayerId} trying to murder {target.PlayerId}");
		GameData.PlayerInfo data = target.Data;
		if (resultFlags.HasFlag(MurderResultFlags.FailedError))
		{
			return;
		}
		if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) || (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) && target.protectedByGuardianId > -1))
		{
			target.protectedByGuardianThisRound = true;
			bool flag = LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
			if (flag && LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsGuardianAngelCrewmatesProtected);
				DestroyableSingleton<AchievementManager>.Instance.OnProtectACrewmate();
			}
			if (AmOwner | flag)
			{
				target.ShowFailedMurder();
				SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
			}
			else
			{
				target.RemoveProtection();
			}
			logger.Debug($"{PlayerId} failed to murder {target.PlayerId} due to guardian angel protection");
		}
		else
		{
			if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) && !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
			{
				return;
			}
			DestroyableSingleton<DebugAnalytics>.Instance.Analytics.Kill(target.Data, Data);
			if (AmOwner)
			{
				if (GameManager.Instance.IsHideAndSeek())
				{
					StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills_HideAndSeek);
				}
				else
				{
					StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
				}
				if (CurrentOutfitType == PlayerOutfitType.Shapeshifted)
				{
					StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
				}
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(KillSfx, loop: false, 0.8f);
				}
				SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
			}
			DestroyableSingleton<UnityTelemetry>.Instance.WriteMurder();
			((Component)target).gameObject.layer = LayerMask.NameToLayer("Ghost");
			if (target.AmOwner)
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
				if (Object.op_Implicit((Object)(object)Minigame.Instance))
				{
					try
					{
						Minigame.Instance.Close();
						Minigame.Instance.Close();
					}
					catch
					{
					}
				}
				DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Data, data);
				target.cosmetics.SetNameMask(alive: false);
				target.RpcSetScanner(value: false);
			}
			DestroyableSingleton<AchievementManager>.Instance.OnMurder(AmOwner, target.AmOwner, CurrentOutfitType == PlayerOutfitType.Shapeshifted, shapeshiftTargetPlayerId, target.PlayerId);
			((MonoBehaviour)MyPhysics).StartCoroutine(KillAnimations.Random().CoPerformKill(this, target));
			logger.Debug($"{PlayerId} succeeded in murdering {target.PlayerId}");
		}
	}

	public void CheckProtect(PlayerControl target)
	{
		logger.Debug($"{PlayerId} trying to protect {target.PlayerId}");
		if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)target) || Data.Role.Role != RoleTypes.GuardianAngel || Data.Role.IsImpostor || Data.Disconnected)
		{
			int num = (Object.op_Implicit((Object)(object)target) ? target.PlayerId : (-1));
			logger.Warning($"Bad protect from {PlayerId} to {num}");
			return;
		}
		GameData.PlayerInfo data = target.Data;
		if (data == null || data.IsDead)
		{
			logger.Warning("Invalid target for protection");
		}
		else
		{
			RpcProtectPlayer(target, Data.DefaultOutfit.ColorId);
		}
	}

	public void ProtectPlayer(PlayerControl target, int colorId)
	{
		logger.Debug($"{PlayerId} protected {target.PlayerId}");
		if (AmOwner)
		{
			Data.Role.SetCooldown();
		}
		target.TurnOnProtection(LocalPlayer.Data.IsDead || (LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor && GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.ImpostorsCanSeeProtect)), colorId, Data.PlayerId);
	}

	public void SetPlayerMaterialColors(Renderer rend)
	{
		PlayerMaterial.SetColors(GameData.Instance.GetPlayerById(PlayerId)?.DefaultOutfit.ColorId ?? 0, rend);
	}

	public static void HideCursorTemporarily()
	{
		if (LocalPlayer.AmOwner)
		{
			((Behaviour)LocalPlayer.MyPhysics.inputHandler).enabled = true;
		}
	}

	public void SetAppearanceFromSaveData()
	{
		MyPhysics.ResetMoveState();
		SetName(DataManager.Player.Customization.Name);
		SetColor(DataManager.Player.Customization.Color);
		SetHat(DataManager.Player.Customization.Hat, DataManager.Player.Customization.Color);
		SetSkin(DataManager.Player.Customization.Skin, DataManager.Player.Customization.Color);
		SetVisor(DataManager.Player.Customization.Visor, DataManager.Player.Customization.Color);
		SetPet(DataManager.Player.Customization.Pet, DataManager.Player.Customization.Color);
		SetNamePlate(DataManager.Player.Customization.NamePlate);
		SpriteAnimNodeSync[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpriteAnimNodeSync>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].LateUpdate();
		}
	}

	public void ToggleHighlight(bool active, RoleTeamTypes targeterTeam)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (active)
		{
			cosmetics.SetOutline(active: true, (targeterTeam == RoleTeamTypes.Crewmate) ? Palette.CrewmateBlue : Palette.ImpostorRed);
		}
		else
		{
			cosmetics.SetOutline(active: false);
		}
	}

	public void SetOutfit(GameData.PlayerOutfit newOutfit, PlayerOutfitType type)
	{
		SetName(newOutfit.PlayerName);
		SetColor(newOutfit.ColorId);
		SetHat(newOutfit.HatId, newOutfit.ColorId);
		SetSkin(newOutfit.SkinId, newOutfit.ColorId);
		SetVisor(newOutfit.VisorId, newOutfit.ColorId);
		SetPet(newOutfit.PetId, newOutfit.ColorId);
		Visible = Visible;
		PlayerOutfitType currentOutfitType = CurrentOutfitType;
		CurrentOutfitType = type;
		if (currentOutfitType != PlayerOutfitType.Default)
		{
			Data.Outfits.Remove(currentOutfitType);
		}
		if (type != PlayerOutfitType.Default)
		{
			Data.SetOutfit(CurrentOutfitType, newOutfit);
		}
	}

	public void RawSetOutfit(GameData.PlayerOutfit newOutfit, PlayerOutfitType type)
	{
		RawSetName(newOutfit.PlayerName);
		RawSetColor(newOutfit.ColorId);
		RawSetHat(newOutfit.HatId, newOutfit.ColorId);
		RawSetSkin(newOutfit.SkinId, newOutfit.ColorId);
		RawSetVisor(newOutfit.VisorId, newOutfit.ColorId);
		RawSetPet(newOutfit.PetId, newOutfit.ColorId);
		Visible = Visible;
		PlayerOutfitType currentOutfitType = CurrentOutfitType;
		CurrentOutfitType = type;
		if (currentOutfitType != PlayerOutfitType.Default)
		{
			Data.Outfits.Remove(currentOutfitType);
		}
		if (type != PlayerOutfitType.Default)
		{
			Data.SetOutfit(CurrentOutfitType, newOutfit);
		}
	}

	public void RejectShapeshift()
	{
		waitingForShapeshiftResponse = false;
	}

	public void Shapeshift(PlayerControl targetPlayer, bool animate)
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		waitingForShapeshiftResponse = false;
		if (CurrentOutfitType == PlayerOutfitType.MushroomMixup)
		{
			logger.Info("Ignoring shapeshift message for " + (((Object)(object)targetPlayer == (Object)null) ? "null player" : targetPlayer.PlayerId.ToString()) + " because of mushroom mixup");
			if (AmOwner)
			{
				DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(Data.Role.Ability);
				Data.Role.SetCooldown();
			}
			return;
		}
		GameData.PlayerInfo targetPlayerInfo = targetPlayer.Data;
		GameData.PlayerOutfit newOutfit;
		if (targetPlayerInfo.PlayerId == Data.PlayerId)
		{
			newOutfit = Data.Outfits[PlayerOutfitType.Default];
		}
		else
		{
			newOutfit = targetPlayer.Data.Outfits[PlayerOutfitType.Default];
		}
		Action changeOutfit = () =>
		{
			if (targetPlayerInfo.PlayerId == Data.PlayerId)
			{
				RawSetOutfit(newOutfit, PlayerOutfitType.Default);
				logger.Info($"Player {PlayerId} Shapeshift is reverting");
				shapeshiftTargetPlayerId = -1;
				if (AmOwner)
				{
					DestroyableSingleton<HudManager>.Instance.AbilityButton.SetFromSettings(Data.Role.Ability);
				}
			}
			else
			{
				RawSetOutfit(newOutfit, PlayerOutfitType.Shapeshifted);
				logger.Info($"Player {PlayerId} is shapeshifting into {targetPlayer.PlayerId}");
				shapeshiftTargetPlayerId = targetPlayer.PlayerId;
				if (AmOwner)
				{
					DestroyableSingleton<HudManager>.Instance.AbilityButton.OverrideText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShapeshiftAbilityUndo));
				}
			}
		};
		if (animate)
		{
			shapeshifting = true;
			MyPhysics.SetNormalizedVelocity(Vector2.zero);
			if (AmOwner && !Object.op_Implicit((Object)(object)Minigame.Instance))
			{
				HideCursorTemporarily();
			}
			RoleEffectAnimation roleEffectAnimation = Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, ((Component)this).gameObject.transform);
			roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(AmOwner);
			roleEffectAnimation.SetMaterialColor(Data.Outfits[PlayerOutfitType.Default].ColorId);
			if (cosmetics.FlipX)
			{
				Transform transform = ((Component)roleEffectAnimation).transform;
				transform.position -= new Vector3(0.14f, 0f, 0f);
			}
			roleEffectAnimation.MidAnimCB = () =>
			{
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				changeOutfit();
				cosmetics.SetScale(MyPhysics.Animations.DefaultPlayerScale, defaultCosmeticsScale);
				(Data.Role as ShapeshifterRole).SetEvidence();
			};
			float shapeshiftScale = MyPhysics.Animations.ShapeshiftScale;
			((MonoBehaviour)this).StartCoroutine(ScalePlayer(shapeshiftScale, 0.25f));
			roleEffectAnimation.Play(this, () =>
			{
				shapeshifting = false;
			}, LocalPlayer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local);
		}
		else
		{
			changeOutfit();
		}
	}

	public void MixUpOutfit(GameData.PlayerOutfit playerOutfit)
	{
		if (Data.IsDead)
		{
			return;
		}
		if (CurrentOutfitType == PlayerOutfitType.Shapeshifted)
		{
			if (AmOwner)
			{
				CmdCheckRevertShapeshift(shouldAnimate: false);
			}
			else
			{
				Shapeshift(this, animate: false);
			}
		}
		RawSetOutfit(playerOutfit, PlayerOutfitType.MushroomMixup);
		RawSetName(Data.DefaultOutfit.PlayerName);
		if (!LocalPlayer.Data.Role.IsImpostor)
		{
			cosmetics.ToggleNameVisible(visible: false);
		}
	}

	public IEnumerator ScalePlayer(float targetScale, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			cosmetics.SetScale(Vector3.Lerp(MyPhysics.Animations.DefaultPlayerScale, MyPhysics.Animations.DefaultPlayerScale * targetScale, elapsedTime / duration), Vector3.Lerp(defaultCosmeticsScale, defaultCosmeticsScale * targetScale, elapsedTime / duration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		cosmetics.SetScale(new Vector3(MyPhysics.Animations.DefaultPlayerScale.x * targetScale, MyPhysics.Animations.DefaultPlayerScale.y * targetScale, 1f), new Vector3(defaultCosmeticsScale.x * targetScale, defaultCosmeticsScale.y * targetScale, 1f));
	}

	public void CmdCheckRevertShapeshift(bool shouldAnimate)
	{
		CmdCheckShapeshift(this, shouldAnimate);
	}

	public void FixMixedUpOutfit()
	{
		if (CurrentOutfitType == PlayerOutfitType.MushroomMixup)
		{
			RawSetOutfit(Data.DefaultOutfit, PlayerOutfitType.Default);
			cosmetics.ToggleNameVisible(GameManager.Instance.LogicOptions.GetShowCrewmateNames());
		}
		PlayerTask playerTask = myTasks.Find((PlayerTask x) => x.TaskType == TaskTypes.MushroomMixupSabotage);
		if ((Object)(object)playerTask != (Object)null)
		{
			RemoveTask(playerTask);
		}
	}

	private void RawSetName(string name)
	{
		((Object)((Component)this).gameObject).name = name;
		cosmetics.SetName(name);
		cosmetics.SetNameMask(alive: true);
	}

	private void RawSetColor(int bodyColor)
	{
		cosmetics.SetColor(bodyColor);
	}

	private void RawSetVisor(string visorId, int color)
	{
		cosmetics.SetVisor(visorId, color);
	}

	private void RawSetSkin(string skinId, int color)
	{
		MyPhysics.SetSkin(skinId, color);
	}

	private void RawSetHat(string hatId, int colorId)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		cosmetics.SetHat(hatId, colorId);
		cosmetics.SetNamePosition(new Vector3(0f, string.IsNullOrEmpty(hatId) ? 0.8f : 1f, -0.5f));
	}

	private void RawSetPet(string petId, int colorId)
	{
		cosmetics.SetPetIdle(petId, colorId, () =>
		{
			cosmetics.SetPetSource(this);
			if (inMovingPlat)
			{
				cosmetics.SetPetVisible(v: false);
			}
		});
	}

	private void ShowFailedMurder()
	{
		RoleEffectAnimation roleEffectAnimation = Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.protectAnim, ((Component)this).gameObject.transform);
		roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(shouldBeVisible: true);
		roleEffectAnimation.Play(this, null, cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
		RemoveProtection();
	}

	private void RemoveProtection()
	{
		for (int num = currentRoleAnimations.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)currentRoleAnimations[num] != (Object)null && currentRoleAnimations[num].effectType == RoleEffectAnimation.EffectType.ProtectLoop)
			{
				Object.Destroy((Object)(object)((Component)currentRoleAnimations[num]).gameObject);
				currentRoleAnimations.RemoveAt(num);
			}
		}
		protectedByGuardianId = -1;
	}

	private void TurnOnProtection(bool visible, int colorId, int guardianPlayerId)
	{
		protectedByGuardianId = guardianPlayerId;
		float duration = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ProtectionDurationSeconds);
		if (visible)
		{
			RoleEffectAnimation roleEffectAnimation = Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.protectLoopAnim, ((Component)this).gameObject.transform);
			roleEffectAnimation.SetMaterialColor(colorId);
			roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(AmOwner);
			roleEffectAnimation.Play(this, RemoveProtection, LocalPlayer.cosmetics.FlipX, (Data.Role.Role == RoleTypes.GuardianAngel) ? RoleEffectAnimation.SoundType.Local : RoleEffectAnimation.SoundType.None, duration);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Wait(duration), Effects.Action(RemoveProtection)));
		}
	}

	private void SetColorBlindTag()
	{
		if (!GameManager.Instance.GameHasStarted || GameManager.Instance.LogicOptions.GetShowCrewmateNames())
		{
			cosmetics.SetEnabledColorblind(DataManager.Settings.Accessibility.ColorBlindMode);
		}
	}

	public bool CanPet()
	{
		if (!DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed && !inVent && !petting && !Data.IsDead)
		{
			return cosmetics.HasPetEquipped();
		}
		return false;
	}

	public void AdjustLighting()
	{
		if ((Object)(object)LocalPlayer != (Object)(object)this)
		{
			return;
		}
		float value = 0f;
		if (IsFlashlightEnabled())
		{
			if (Data.Role.IsImpostor)
			{
				GameOptionsManager.Instance.CurrentGameOptions.TryGetFloat(FloatOptionNames.ImpostorFlashlightSize, out value);
			}
			else
			{
				GameOptionsManager.Instance.CurrentGameOptions.TryGetFloat(FloatOptionNames.CrewmateFlashlightSize, out value);
			}
		}
		SetFlashlightInputMethod();
		lightSource.SetupLightingForGameplay(IsFlashlightEnabled(), value, TargetFlashlight.transform);
	}

	private bool IsFlashlightEnabled()
	{
		if ((Object)(object)LobbyBehaviour.Instance != (Object)null)
		{
			return false;
		}
		if (LocalPlayer.Data.IsDead)
		{
			return false;
		}
		if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek)
		{
			return false;
		}
		bool value = false;
		if (!GameOptionsManager.Instance.CurrentGameOptions.TryGetBool(BoolOptionNames.UseFlashlight, out value))
		{
			return false;
		}
		return value;
	}

	private void SetFlashlightInputMethod()
	{
		if ((Object)(object)LocalPlayer != (Object)(object)this)
		{
			return;
		}
		if (!IsFlashlightEnabled())
		{
			TargetFlashlight.gameObject.SetActive(false);
			((MonoBehaviour)this).StartCoroutine(EnableRightJoystick(enabled: false));
		}
		else if (DataManager.Settings.Input.InputMode == ControlTypes.ScreenJoystick)
		{
			GameObject targetFlashlight = TargetFlashlight;
			if (targetFlashlight != null)
			{
				targetFlashlight.gameObject.SetActive(true);
			}
			((MonoBehaviour)this).StartCoroutine(EnableRightJoystick(enabled: false));
		}
		else
		{
			GameObject targetFlashlight2 = TargetFlashlight;
			if (targetFlashlight2 != null)
			{
				targetFlashlight2.gameObject.SetActive(false);
			}
			((MonoBehaviour)this).StartCoroutine(EnableRightJoystick(enabled: true));
		}
	}

	private IEnumerator EnableRightJoystick(bool enabled)
	{
		yield return (object)new WaitForEndOfFrame();
		while (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			yield return null;
		}
		DestroyableSingleton<HudManager>.Instance.ToggleRightJoystick(enabled);
	}

	public PlayerControl()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
	}
}
