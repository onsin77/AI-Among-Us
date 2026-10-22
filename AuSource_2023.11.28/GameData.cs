using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using UnityEngine;

[SkipRename]
public class GameData : InnerNetObject, IDisconnectHandler
{
	public class TaskInfo
	{
		public uint Id;

		public byte TypeId;

		public bool Complete;

		public TaskInfo()
		{
		}

		public TaskInfo(byte typeId, uint id)
		{
			Id = id;
			TypeId = typeId;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.WritePacked(Id);
			writer.Write(Complete);
		}

		public void Deserialize(MessageReader reader)
		{
			Id = reader.ReadPackedUInt32();
			Complete = reader.ReadBoolean();
		}
	}

	public class PlayerInfo
	{
		public readonly byte PlayerId;

		public string FriendCode;

		public string Puid;

		public RoleTypes RoleType;

		public RoleTypes? RoleWhenAlive;

		public Dictionary<PlayerOutfitType, PlayerOutfit> Outfits = new Dictionary<PlayerOutfitType, PlayerOutfit> { 
		{
			PlayerOutfitType.Default,
			new PlayerOutfit()
		} };

		public uint PlayerLevel = uint.MaxValue;

		public bool Disconnected;

		public RoleBehaviour Role;

		public List<TaskInfo> Tasks;

		public bool IsDead;

		private PlayerControl _object;

		public bool IsIncomplete
		{
			get
			{
				if (Outfits.Count != 0 && !Outfits.Values.Any((PlayerOutfit outfit) => outfit.IsIncomplete))
				{
					return PlayerLevel == uint.MaxValue;
				}
				return true;
			}
		}

		public PlayerControl Object
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)_object))
				{
					_object = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl p) => p.PlayerId == PlayerId);
				}
				return _object;
			}
		}

		public PlayerOutfit DefaultOutfit => Outfits[PlayerOutfitType.Default];

		public string PlayerName
		{
			get
			{
				return GetPlayerName(PlayerOutfitType.Default);
			}
			set
			{
				DefaultOutfit.PlayerName = value;
			}
		}

		public Color Color
		{
			get
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return Color32.op_Implicit(Palette.PlayerColors[Outfits[PlayerOutfitType.Default].ColorId]);
			}
		}

		public Color ShadowColor
		{
			get
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return Color32.op_Implicit(Palette.ShadowColors[Outfits[PlayerOutfitType.Default].ColorId]);
			}
		}

		public string ColorName => "(" + GetPlayerColorString() + ")";

		public PlayerInfo(byte playerId)
		{
			PlayerId = playerId;
		}

		public PlayerInfo(byte playerId, string friendCode, string puid)
		{
			PlayerId = playerId;
			FriendCode = friendCode;
			Puid = puid;
		}

		public PlayerInfo(PlayerControl pc)
			: this(pc.PlayerId, pc.FriendCode, pc.Puid)
		{
			_object = pc;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.Write((byte)Outfits.Count);
			foreach (KeyValuePair<PlayerOutfitType, PlayerOutfit> outfit in Outfits)
			{
				writer.Write((byte)outfit.Key);
				outfit.Value.Serialize(writer);
			}
			writer.WritePacked(PlayerLevel);
			byte b = 0;
			if (Disconnected)
			{
				b |= 1;
			}
			if (IsDead)
			{
				b |= 4;
			}
			writer.Write(b);
			writer.Write((ushort)Role.Role);
			writer.Write(RoleWhenAlive.HasValue);
			if (RoleWhenAlive.HasValue)
			{
				writer.Write((ushort)RoleWhenAlive.Value);
			}
			if (Tasks != null)
			{
				writer.Write((byte)Tasks.Count);
				for (int i = 0; i < Tasks.Count; i++)
				{
					Tasks[i].Serialize(writer);
				}
			}
			else
			{
				writer.Write((byte)0);
			}
			writer.Write(FriendCode ?? string.Empty);
			writer.Write(Puid ?? string.Empty);
		}

		public void Deserialize(MessageReader reader)
		{
			byte b = reader.ReadByte();
			Outfits.Clear();
			for (int i = 0; i < b; i++)
			{
				PlayerOutfitType key = (PlayerOutfitType)reader.ReadByte();
				Outfits[key] = new PlayerOutfit();
				Outfits[key].Deserialize(reader);
			}
			PlayerLevel = reader.ReadPackedUInt32();
			byte b2 = reader.ReadByte();
			Disconnected = (b2 & 1) != 0;
			IsDead = (b2 & 4) != 0;
			RoleType = (RoleTypes)reader.ReadUInt16();
			if (reader.ReadBoolean())
			{
				RoleWhenAlive = (RoleTypes)reader.ReadUInt16();
			}
			byte b3 = reader.ReadByte();
			Tasks = new List<TaskInfo>(b3);
			for (int j = 0; j < b3; j++)
			{
				Tasks.Add(new TaskInfo());
				Tasks[j].Deserialize(reader);
			}
			FriendCode = reader.ReadString();
			Puid = reader.ReadString();
		}

		public string GetPlayerName(PlayerOutfitType outfitType)
		{
			if (!Outfits.ContainsKey(outfitType))
			{
				return string.Empty;
			}
			return Outfits[outfitType].PlayerName;
		}

		public string GetPlayerNameAsync(PlayerOutfitType outfitType, Action<string> callback)
		{
			if (!Outfits.ContainsKey(outfitType))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(Outfits[outfitType].PlayerName))
			{
				Outfits[outfitType].nameCallback = callback;
				return string.Empty;
			}
			return Outfits[outfitType].PlayerName;
		}

		public string GetPlayerColorString(PlayerOutfitType outfitType = PlayerOutfitType.Default)
		{
			if (!Outfits.ContainsKey(outfitType))
			{
				return string.Empty;
			}
			char[] array = Palette.GetColorName(Outfits[outfitType].ColorId).ToCharArray();
			if (array.Length != 0)
			{
				array[0] = char.ToUpper(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					array[i] = char.ToLower(array[i]);
				}
			}
			return new string(array);
		}

		public TaskInfo FindTaskById(uint taskId)
		{
			for (int i = 0; i < Tasks.Count; i++)
			{
				if (Tasks[i].Id == taskId)
				{
					return Tasks[i];
				}
			}
			return null;
		}

		public void SetOutfit(PlayerOutfitType outfitType, PlayerOutfit outfit)
		{
			Outfits[outfitType] = outfit;
		}

		public void OnGameStart()
		{
			Object?.OnGameStart();
		}

		public void OnGameEnd()
		{
			Object?.OnGameEnd();
		}
	}

	public class PlayerOutfit
	{
		public bool dontCensorName;

		public int ColorId = -1;

		public string HatId = "missing";

		public string PetId = "missing";

		public string SkinId = "missing";

		public string VisorId = "missing";

		public string NamePlateId = "missing";

		public Action<string> nameCallback;

		private string preCensorName = string.Empty;

		private string postCensorName = string.Empty;

		public string PlayerName
		{
			get
			{
				return postCensorName;
			}
			set
			{
				postCensorName = (preCensorName = value);
			}
		}

		public bool IsIncomplete
		{
			get
			{
				if (!string.IsNullOrEmpty(PlayerName) && ColorId != -1 && !(HatId == "missing") && !(PetId == "missing") && !(SkinId == "missing") && !(VisorId == "missing"))
				{
					return NamePlateId == "missing";
				}
				return true;
			}
		}

		public void Serialize(MessageWriter writer)
		{
			writer.Write(preCensorName);
			writer.WritePacked(ColorId);
			writer.Write(HatId);
			writer.Write(PetId);
			writer.Write(SkinId);
			writer.Write(VisorId);
			writer.Write(NamePlateId);
		}

		public void Deserialize(MessageReader reader)
		{
			PlayerName = reader.ReadString();
			ColorId = reader.ReadPackedInt32();
			HatId = reader.ReadString();
			PetId = reader.ReadString();
			SkinId = reader.ReadString();
			VisorId = reader.ReadString();
			NamePlateId = reader.ReadString();
		}
	}

	public static GameData Instance;

	public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();

	public int TotalTasks;

	public int CompletedTasks;

	public RoleBehaviour DefaultRole;

	public const byte InvalidPlayerId = byte.MaxValue;

	public const byte DisconnectedPlayerId = 254;

	public static Random randy = new Random(0);

	public override bool Chunked => true;

	public int PlayerCount => AllPlayers.Count;

	public void RpcSetTasks(byte playerId, byte[] taskTypeIds)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetTasks(playerId, taskTypeIds);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 29, (SendOption)1);
		val.Write(playerId);
		val.WriteBytesAndSize(taskTypeIds);
		val.EndMessage();
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		List<PlayerInfo> list = AllPlayers.OrderBy((PlayerInfo p) => (!string.IsNullOrWhiteSpace(p.PlayerName)) ? 1 : 0).ToList();
		int num = 0;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			PlayerInfo playerInfo = list[num2];
			if (!initialState && !IsDirtyBitSet(playerInfo.PlayerId))
			{
				continue;
			}
			if (!initialState)
			{
				UnsetDirtyBit((uint)(1 << (int)playerInfo.PlayerId));
			}
			writer.StartMessage(playerInfo.PlayerId);
			playerInfo.Serialize(writer);
			writer.EndMessage();
			if (++num <= 4)
			{
				continue;
			}
			if (initialState)
			{
				for (int num3 = num2 + 1; num3 < list.Count; num3++)
				{
					PlayerInfo playerInfo2 = list[num3];
					SetDirtyBit((uint)(1 << (int)playerInfo2.PlayerId));
				}
			}
			break;
		}
		if (num == 0)
		{
			ClearDirtyBits();
			return false;
		}
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		while (reader.Position < reader.Length)
		{
			MessageReader val = reader.ReadMessage();
			PlayerInfo playerById = GetPlayerById(val.Tag);
			if (playerById != null)
			{
				playerById.Deserialize(val);
				continue;
			}
			playerById = new PlayerInfo(val.Tag);
			playerById.Deserialize(val);
			AllPlayers.Add(playerById);
		}
		foreach (PlayerInfo allPlayer in AllPlayers)
		{
			AmongUsClient.Instance.GetClientFromCharacter(allPlayer.Object)?.UpdatePlayerName(allPlayer.GetPlayerName(PlayerOutfitType.Default));
			if (!Object.op_Implicit((Object)(object)allPlayer.Role))
			{
				DestroyableSingleton<RoleManager>.Instance.SetRole(allPlayer.Object, allPlayer.RoleType);
			}
		}
		RecomputeTaskCounts();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId == 29)
		{
			SetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
		}
	}

	public void Awake()
	{
		if (Object.op_Implicit((Object)(object)Instance) && (Object)(object)Instance != (Object)(object)this)
		{
			Debug.Log((object)"Destroying dupe GameData");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		}
	}

	internal void SetDirty()
	{
		SetDirtyBit(uint.MaxValue);
	}

	public PlayerInfo GetHost()
	{
		ClientData host = AmongUsClient.Instance.GetHost();
		if (host != null && Object.op_Implicit((Object)(object)host.Character))
		{
			return host.Character.Data;
		}
		return null;
	}

	public sbyte GetAvailableId()
	{
		sbyte i;
		for (i = 0; i < sbyte.MaxValue; i++)
		{
			if (!AllPlayers.Any((PlayerInfo p) => p.PlayerId == i))
			{
				return i;
			}
		}
		return -1;
	}

	public PlayerInfo GetPlayerById(byte id)
	{
		if (id == byte.MaxValue)
		{
			return null;
		}
		for (int i = 0; i < AllPlayers.Count; i++)
		{
			if (AllPlayers[i].PlayerId == id)
			{
				return AllPlayers[i];
			}
		}
		return null;
	}

	public void UpdateName(byte playerId, string name, bool dontCensor = false)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			PlayerOutfit defaultOutfit = playerById.DefaultOutfit;
			defaultOutfit.dontCensorName = dontCensor;
			defaultOutfit.PlayerName = name;
			AmongUsClient.Instance.GetClientFromCharacter(playerById.Object)?.UpdatePlayerName(name);
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateLevel(byte playerId, uint level)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.PlayerLevel = level;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateColor(byte playerId, int color)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.ColorId = color;
			ClientData clientFromCharacter = AmongUsClient.Instance.GetClientFromCharacter(playerById.Object);
			if (clientFromCharacter != null)
			{
				clientFromCharacter.ColorId = color;
			}
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateHat(byte playerId, string hat)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.HatId = hat;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdatePet(byte playerId, string petId)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.PetId = petId;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateSkin(byte playerId, string skin)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.SkinId = skin;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateVisor(byte playerId, string visor)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.VisorId = visor;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public void UpdateNamePlate(byte playerId, string namePlate)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.DefaultOutfit.NamePlateId = namePlate;
		}
		SetDirtyBit((uint)(1 << (int)playerId));
	}

	public PlayerInfo AddPlayer(PlayerControl pc)
	{
		PlayerInfo playerInfo = new PlayerInfo(pc);
		playerInfo.Role = Object.Instantiate<RoleBehaviour>(DefaultRole);
		playerInfo.Role.Initialize(pc);
		AllPlayers.Add(playerInfo);
		SetDirtyBit((uint)(1 << (int)pc.PlayerId));
		return playerInfo;
	}

	public bool RemovePlayer(byte playerId)
	{
		for (int i = 0; i < AllPlayers.Count; i++)
		{
			if (AllPlayers[i].PlayerId == playerId)
			{
				SetDirty();
				AllPlayers.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public void RecomputeTaskCounts()
	{
		if ((Object)(object)GameManager.Instance == (Object)null)
		{
			return;
		}
		TotalTasks = 0;
		CompletedTasks = 0;
		for (int i = 0; i < AllPlayers.Count; i++)
		{
			PlayerInfo playerInfo = AllPlayers[i];
			if (playerInfo.Disconnected || playerInfo.Tasks == null || !Object.op_Implicit((Object)(object)playerInfo.Object) || (!GameManager.Instance.LogicOptions.GetGhostsDoTasks() && playerInfo.IsDead) || !Object.op_Implicit((Object)(object)playerInfo.Role) || !playerInfo.Role.TasksCountTowardProgress)
			{
				continue;
			}
			for (int j = 0; j < playerInfo.Tasks.Count; j++)
			{
				TotalTasks++;
				if (playerInfo.Tasks[j].Complete)
				{
					CompletedTasks++;
				}
			}
		}
	}

	public void TutOnlyRemoveTask(byte playerId, uint taskId)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		TaskInfo item = playerById.FindTaskById(taskId);
		playerById.Tasks.Remove(item);
		RecomputeTaskCounts();
	}

	public uint TutOnlyAddTask(byte playerId)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		uint num = playerById.Tasks.Select((TaskInfo d) => d.Id).Max() + 1;
		playerById.Tasks.Add(new TaskInfo
		{
			Id = num
		});
		TotalTasks++;
		return num;
	}

	private void SetTasks(byte playerId, byte[] taskTypeIds)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById == null)
		{
			Debug.Log((object)("Could not set tasks for player id: " + playerId));
		}
		else
		{
			if (playerById.Disconnected)
			{
				return;
			}
			if (!Object.op_Implicit((Object)(object)playerById.Object))
			{
				Debug.Log((object)("Could not set tasks for player (" + playerById.PlayerName + "): " + playerId));
				return;
			}
			if (GameManager.Instance.IsHideAndSeek() && playerById.RoleType == RoleTypes.Impostor)
			{
				taskTypeIds = new byte[0];
			}
			playerById.Tasks = new List<TaskInfo>(taskTypeIds.Length);
			for (int i = 0; i < taskTypeIds.Length; i++)
			{
				playerById.Tasks.Add(new TaskInfo(taskTypeIds[i], (uint)i));
				playerById.Tasks[i].Id = (uint)i;
			}
			playerById.Object.SetTasks(playerById.Tasks);
			SetDirtyBit((uint)(1 << (int)playerById.PlayerId));
		}
	}

	public void CompleteTask(PlayerControl pc, uint taskId)
	{
		TaskInfo taskInfo = pc.Data.FindTaskById(taskId);
		if (taskInfo != null)
		{
			if (!taskInfo.Complete)
			{
				taskInfo.Complete = true;
				CompletedTasks++;
			}
			else
			{
				Debug.LogWarning((object)("Double complete task: " + taskId));
			}
		}
		else
		{
			Debug.LogWarning((object)("Couldn't find task: " + taskId));
		}
	}

	public void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
	{
		if (!Object.op_Implicit((Object)(object)player))
		{
			return;
		}
		PlayerInfo playerById = GetPlayerById(player.PlayerId);
		if (playerById == null)
		{
			return;
		}
		if (AmongUsClient.Instance.IsGameStarted)
		{
			if (!playerById.Disconnected)
			{
				playerById.Disconnected = true;
				TempData.LastDeathReason = DeathReason.Disconnect;
				ShowNotification(playerById.PlayerName, reason);
			}
		}
		else if (RemovePlayer(player.PlayerId))
		{
			ShowNotification(playerById.PlayerName, reason);
		}
		RecomputeTaskCounts();
	}

	private void ShowNotification(string playerName, DisconnectReasons reason)
	{
		if (string.IsNullOrEmpty(playerName))
		{
			return;
		}
		if (reason <= DisconnectReasons.Banned)
		{
			switch (reason)
			{
			case DisconnectReasons.ExitGame:
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UserLeftGame, playerName));
				return;
			case DisconnectReasons.Banned:
			{
				PlayerInfo data = AmongUsClient.Instance.GetHost().Character.Data;
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerWasBannedBy, playerName, data.PlayerName));
				return;
			}
			}
		}
		else
		{
			if (reason == DisconnectReasons.Kicked)
			{
				PlayerInfo data2 = AmongUsClient.Instance.GetHost().Character.Data;
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerWasKickedBy, playerName, data2.PlayerName));
				return;
			}
			_ = 17;
		}
		DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LeftGameError, playerName));
	}

	public void HandleDisconnect()
	{
		if (AmongUsClient.Instance.IsGameStarted)
		{
			return;
		}
		for (int num = AllPlayers.Count - 1; num >= 0; num--)
		{
			if (!Object.op_Implicit((Object)(object)AllPlayers[num].Object))
			{
				AllPlayers.RemoveAt(num);
			}
		}
	}
}
