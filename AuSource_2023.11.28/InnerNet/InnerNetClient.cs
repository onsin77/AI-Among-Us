using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using Hazel.Udp;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace InnerNet;

public abstract class InnerNetClient : MonoBehaviour
{
	public enum GameStates
	{
		NotJoined,
		Joined,
		Started,
		Ended
	}

	public class TotalGameData
	{
		public int[] PerMapTotals;
	}

	private class RecentPlayerComparer : IComparer<ClientData>
	{
		public static readonly RecentPlayerComparer Instance = new RecentPlayerComparer();

		public int Compare(ClientData x, ClientData y)
		{
			if (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			int num = ((AmongUsClient.Instance.GetClient(x.Id) != null) ? 1 : 0);
			int num2 = ((AmongUsClient.Instance.GetClient(y.Id) != null) ? 1 : 0);
			if (num != num2)
			{
				return num - num2;
			}
			return y.PlayerName.CompareTo(x.PlayerName);
		}
	}

	public const int CurrentClient = -3;

	public const int HostInherit = -2;

	internal const byte DataFlag = 1;

	internal const byte RpcFlag = 2;

	internal const byte SpawnFlag = 4;

	internal const byte DespawnFlag = 5;

	internal const byte SceneChangeFlag = 6;

	internal const byte ReadyFlag = 7;

	internal const byte ChangeSettingsFlag = 8;

	internal const byte XboxDeclareXuid = 207;

	public float MinSendInterval = 0.1f;

	private uint NetIdCnt = 1u;

	private float timer;

	public AssetReference[] SpawnableObjects;

	public InnerNetObject[] NonAddressableSpawnableObjects;

	private bool InOnlineScene;

	private HashSet<uint> DestroyedObjects = new HashSet<uint>();

	public List<InnerNetObject> allObjects = new List<InnerNetObject>();

	private Dictionary<uint, InnerNetObject> allObjectsFast = new Dictionary<uint, InnerNetObject>();

	private MessageWriter[] Streams;

	private int msgNum;

	private static readonly HashSet<DisconnectReasons> DontBotherLoggingTheseDisconnectReasons = new HashSet<DisconnectReasons>
	{
		DisconnectReasons.ExitGame,
		DisconnectReasons.GameFull,
		DisconnectReasons.GameStarted,
		DisconnectReasons.GameNotFound,
		DisconnectReasons.ConnectionLimit,
		DisconnectReasons.QuickchatLock
	};

	public const int MaxRecentClients = 20;

	private const int DefaultSecondsSuspendedBeforeDisconnect = 30;

	public static int SecondsSuspendedBeforeDisconnect = 30;

	public const int NoClientId = -1;

	private readonly Logger serverLogger = new Logger("Server", Logger.Level.Debug, Logger.Category.Server);

	private string networkAddress = "127.0.0.1";

	private int networkPort;

	private bool useDtls;

	private UnityUdpClientConnection connection;

	public MatchMakerModes mode;

	public NetworkModes NetworkMode;

	public int GameId = 32;

	public int HostId;

	public int ClientId = -1;

	protected List<ClientData> allClients = new List<ClientData>();

	protected CircleBuffer<ClientData> recentClients = new CircleBuffer<ClientData>(20);

	public DisconnectReasons LastDisconnectReason;

	public string LastCustomDisconnect;

	public byte LastServerChatMode;

	public HttpMatchmakerManager.MatchmakerError LastMatchmakerError;

	private readonly List<Action> PreSpawnDispatcher = new List<Action>();

	private readonly List<Action> Dispatcher = new List<Action>();

	private readonly GameOptionsFactory gameOptionsFactory = new GameOptionsFactory((ILogger)(object)new UnityLogger());

	public GameStates GameState;

	private bool isConnecting;

	private bool platformSpecificsChecked;

	private List<Action> TempQueue = new List<Action>();

	private volatile bool appPaused;

	public bool AmConnected => connection != null;

	public int Ping
	{
		get
		{
			if (connection == null)
			{
				return 0;
			}
			return (int)((NetworkConnection)connection).AveragePingMs;
		}
	}

	public int BytesSent
	{
		get
		{
			if (connection == null)
			{
				return 0;
			}
			return (int)((Connection)connection).Statistics.TotalBytesSent;
		}
	}

	public int BytesGot
	{
		get
		{
			if (connection == null)
			{
				return 0;
			}
			return (int)((Connection)connection).Statistics.TotalBytesReceived;
		}
	}

	public int Resends
	{
		get
		{
			if (connection == null)
			{
				return 0;
			}
			return ((Connection)connection).Statistics.MessagesResent;
		}
	}

	public bool AmHost => HostId == ClientId;

	public bool AmLocalHost
	{
		get
		{
			if (!AmHost || AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame)
			{
				return DestroyableSingleton<TutorialManager>.InstanceExists;
			}
			return true;
		}
	}

	public bool AmModdedHost
	{
		get
		{
			if (AmHost)
			{
				return Constants.IsVersionModded();
			}
			return false;
		}
	}

	public bool AmClient => ClientId > 0;

	public bool IsGamePublic { get; private set; }

	public bool IsGameStarted => GameState == GameStates.Started;

	public bool IsGameOver => GameState == GameStates.Ended;

	public bool IsInGame => GameState != GameStates.NotJoined;

	private void FixedUpdate()
	{
		if (connection != null)
		{
			connection.FixedUpdate();
		}
		if (mode == MatchMakerModes.None || Streams == null)
		{
			timer = 0f;
			return;
		}
		timer += Time.fixedDeltaTime;
		if (!(timer < MinSendInterval))
		{
			timer = 0f;
			while (SendAllStreamedObjects())
			{
			}
		}
	}

	protected void UpdateCachedClients(ClientData clientData, PlayerControl character)
	{
		ClientData client = GetClient(clientData.Id);
		if (client != null)
		{
			client.Character = character;
		}
		ClientData recentClient = GetRecentClient(clientData.Id);
		if (recentClient != null)
		{
			recentClient.Character = character;
		}
	}

	private bool SendAllStreamedObjects()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		lock (allObjects)
		{
			for (int i = 0; i < allObjects.Count; i++)
			{
				InnerNetObject innerNetObject = allObjects[i];
				if (!Object.op_Implicit((Object)(object)innerNetObject) || !innerNetObject.IsDirty || (!innerNetObject.AmOwner && (innerNetObject.OwnerId != -2 || !AmHost)))
				{
					continue;
				}
				MessageWriter val = Streams[innerNetObject.sendMode];
				val.StartMessage((byte)1);
				val.WritePacked(innerNetObject.NetId);
				try
				{
					if (innerNetObject.Serialize(val, initialState: false))
					{
						val.EndMessage();
					}
					else
					{
						val.CancelMessage();
					}
					if (innerNetObject.Chunked && innerNetObject.IsDirty)
					{
						result = true;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					val.CancelMessage();
				}
			}
		}
		for (int j = 0; j < Streams.Length; j++)
		{
			MessageWriter val2 = Streams[j];
			if (val2.HasBytes(7))
			{
				val2.EndMessage();
				SendOrDisconnect(val2);
				val2.Clear((SendOption)(byte)j);
				val2.StartMessage((byte)5);
				val2.Write(GameId);
			}
		}
		return result;
	}

	public T FindObjectByNetId<T>(uint netId) where T : InnerNetObject
	{
		if (allObjectsFast.TryGetValue(netId, out var value))
		{
			return (T)value;
		}
		return null;
	}

	public MessageWriter StartRpcImmediately(uint targetNetId, byte callId, SendOption option, int targetClientId = -1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MessageWriter val = MessageWriter.Get(option);
		if (targetClientId < 0)
		{
			val.StartMessage((byte)5);
			val.Write(GameId);
		}
		else
		{
			val.StartMessage((byte)6);
			val.Write(GameId);
			val.WritePacked(targetClientId);
		}
		val.StartMessage((byte)2);
		val.WritePacked(targetNetId);
		val.Write(callId);
		return val;
	}

	public void FinishRpcImmediately(MessageWriter msg)
	{
		msg.EndMessage();
		msg.EndMessage();
		SendOrDisconnect(msg);
		msg.Recycle();
	}

	public void SendXuidToAll()
	{
		Debug.Log((object)"InnerNetClient.During::SendXuidToAll");
	}

	public void SendRpc(uint targetNetId, byte callId, SendOption option = (SendOption)1)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		StartRpc(targetNetId, callId, option).EndMessage();
	}

	public MessageWriter StartRpc(uint targetNetId, byte callId, SendOption option = (SendOption)1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MessageWriter obj = Streams[option];
		obj.StartMessage((byte)2);
		obj.WritePacked(targetNetId);
		obj.Write(callId);
		return obj;
	}

	private void SendSceneChange(string sceneName)
	{
		if (!(sceneName == "FindAGame"))
		{
			InOnlineScene = string.Equals(sceneName, "OnlineGame");
			if (AmConnected)
			{
				Debug.Log((object)("Changed To " + sceneName));
				((MonoBehaviour)this).StartCoroutine(CoSendSceneChange(sceneName));
			}
		}
	}

	private IEnumerator CoSendSceneChange(string sceneName)
	{
		DestroyableSingleton<FriendsListManager>.Instance.ReparentUI();
		lock (allObjects)
		{
			for (int num = allObjects.Count - 1; num > -1; num--)
			{
				if (!Object.op_Implicit((Object)(object)allObjects[num]))
				{
					allObjects.RemoveAt(num);
				}
			}
		}
		while (AmConnected && ClientId < 0)
		{
			yield return null;
		}
		if (AmConnected)
		{
			ClientData clientData = FindClientById(ClientId);
			if (clientData != null)
			{
				Debug.Log((object)$"Self changed scene: {ClientId} {sceneName}");
				yield return CoOnPlayerChangedScene(clientData, sceneName);
			}
			else
			{
				Debug.Log((object)($"Couldn't find self in clients: {ClientId}: " + sceneName));
			}
			if (!AmHost && (int)((Connection)connection).State == 2)
			{
				MessageWriter val = MessageWriter.Get((SendOption)1);
				val.StartMessage((byte)5);
				val.Write(GameId);
				val.StartMessage((byte)6);
				val.WritePacked(ClientId);
				val.Write(sceneName);
				val.EndMessage();
				val.EndMessage();
				SendOrDisconnect(val);
				val.Recycle();
			}
		}
	}

	public void Spawn(InnerNetObject netObjParent, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
	{
		if (!AmHost)
		{
			if (AmClient)
			{
				Debug.LogError((object)("Tried to spawn while not host:" + (object)netObjParent));
			}
		}
		else
		{
			ownerId = ((ownerId == -3) ? ClientId : ownerId);
			MessageWriter msg = Streams[1];
			WriteSpawnMessage(netObjParent, ownerId, flags, msg);
		}
	}

	private void WriteSpawnMessage(InnerNetObject netObjParent, int ownerId, SpawnFlags flags, MessageWriter msg)
	{
		msg.StartMessage((byte)4);
		msg.WritePacked(netObjParent.SpawnId);
		msg.WritePacked(ownerId);
		msg.Write((byte)flags);
		InnerNetObject[] componentsInChildren = ((Component)netObjParent).GetComponentsInChildren<InnerNetObject>();
		msg.WritePacked(componentsInChildren.Length);
		foreach (InnerNetObject innerNetObject in componentsInChildren)
		{
			innerNetObject.OwnerId = ownerId;
			innerNetObject.SpawnFlags = flags;
			if (innerNetObject.NetId == 0)
			{
				innerNetObject.NetId = NetIdCnt++;
				allObjects.Add(innerNetObject);
				allObjectsFast.Add(innerNetObject.NetId, innerNetObject);
			}
			msg.WritePacked(innerNetObject.NetId);
			msg.StartMessage((byte)1);
			innerNetObject.Serialize(msg, initialState: true);
			msg.EndMessage();
		}
		msg.EndMessage();
	}

	public void Despawn(InnerNetObject objToDespawn)
	{
		if (objToDespawn.NetId < 1)
		{
			Debug.LogError((object)("Tried to net destroy: " + (object)objToDespawn));
			return;
		}
		MessageWriter obj = Streams[1];
		obj.StartMessage((byte)5);
		obj.WritePacked(objToDespawn.NetId);
		obj.EndMessage();
		RemoveNetObject(objToDespawn);
	}

	private bool AddNetObject(InnerNetObject obj)
	{
		uint num = obj.NetId + 1;
		if (num > NetIdCnt)
		{
			NetIdCnt = num;
		}
		if (!allObjectsFast.ContainsKey(obj.NetId))
		{
			allObjects.Add(obj);
			allObjectsFast.Add(obj.NetId, obj);
			return true;
		}
		return false;
	}

	public void RemoveNetObject(InnerNetObject obj)
	{
		int num = allObjects.IndexOf(obj);
		if (num > -1)
		{
			allObjects.RemoveAt(num);
		}
		allObjectsFast.Remove(obj.NetId);
		obj.NetId = uint.MaxValue;
	}

	public void RemoveUnownedObjects()
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.Add(-2);
		lock (allClients)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if (Object.op_Implicit((Object)(object)clientData.Character))
				{
					hashSet.Add(clientData.Id);
				}
			}
		}
		lock (allObjects)
		{
			for (int num = allObjects.Count - 1; num > -1; num--)
			{
				InnerNetObject innerNetObject = allObjects[num];
				if (!Object.op_Implicit((Object)(object)innerNetObject))
				{
					allObjects.RemoveAt(num);
				}
				else if (!hashSet.Contains(innerNetObject.OwnerId))
				{
					innerNetObject.OwnerId = ClientId;
					Object.Destroy((Object)(object)((Component)innerNetObject).gameObject);
				}
			}
		}
	}

	private void HandleGameData(MessageReader parentReader)
	{
		try
		{
			while (parentReader.Position < parentReader.Length)
			{
				MessageReader reader = parentReader.ReadMessageAsNewBuffer();
				((MonoBehaviour)this).StartCoroutine(HandleGameDataInner(reader, msgNum++));
			}
		}
		finally
		{
			parentReader.Recycle();
		}
	}

	private IEnumerator HandleGameDataInner(MessageReader reader, int msgNum)
	{
		int cnt = 0;
		reader.Position = 0;
		switch (reader.Tag)
		{
		case 1:
			try
			{
				while (true)
				{
					uint num3 = reader.ReadPackedUInt32();
					if (allObjectsFast.TryGetValue(num3, out var value2))
					{
						value2.Deserialize(reader, initialState: false);
						break;
					}
					if (!DestroyedObjects.Contains(num3))
					{
						Debug.LogWarning((object)("Stored data for " + num3));
						if (cnt++ <= 10)
						{
							reader.Position = 0;
							yield return Effects.Wait(0.1f);
							continue;
						}
						break;
					}
					break;
				}
				break;
			}
			finally
			{
				reader.Recycle();
			}
		case 2:
			try
			{
				while (true)
				{
					uint num2;
					try
					{
						num2 = reader.ReadPackedUInt32();
					}
					catch (Exception ex)
					{
						Debug.LogError((object)$"Error in {msgNum} try {cnt}, Pos:{reader.Position}/{reader.Length}: {ex}");
						Debug.LogException(ex, (Object)(object)this);
						throw;
					}
					byte b = reader.ReadByte();
					if (allObjectsFast.TryGetValue(num2, out var value))
					{
						value.HandleRpc(b, reader);
						break;
					}
					if (num2 != uint.MaxValue && !DestroyedObjects.Contains(num2))
					{
						Debug.LogWarning((object)($"Stored Msg {msgNum} RPC {(RpcCalls)b} for " + num2));
						if (cnt++ <= 10)
						{
							reader.Position = 0;
							yield return Effects.Wait(0.1f);
							continue;
						}
						break;
					}
					break;
				}
				break;
			}
			finally
			{
				reader.Recycle();
			}
		case 4:
			((MonoBehaviour)this).StartCoroutine(CoHandleSpawn(reader));
			break;
		case 5:
			try
			{
				uint num4 = reader.ReadPackedUInt32();
				DestroyedObjects.Add(num4);
				InnerNetObject innerNetObject = FindObjectByNetId<InnerNetObject>(num4);
				if (Object.op_Implicit((Object)(object)innerNetObject) && !innerNetObject.AmOwner)
				{
					RemoveNetObject(innerNetObject);
					Object.Destroy((Object)(object)((Component)innerNetObject).gameObject);
				}
				break;
			}
			finally
			{
				reader.Recycle();
			}
		case 6:
		{
			int num = reader.ReadPackedInt32();
			ClientData clientData2 = FindClientById(num);
			string text = reader.ReadString();
			if (clientData2 != null && !string.IsNullOrWhiteSpace(text))
			{
				((MonoBehaviour)this).StartCoroutine(CoOnPlayerChangedScene(clientData2, text));
				break;
			}
			Debug.Log((object)$"Couldn't find client {num} to change scene to {text}");
			reader.Recycle();
			break;
		}
		case 7:
			try
			{
				ClientData clientData = FindClientById(reader.ReadPackedInt32());
				if (clientData != null)
				{
					clientData.IsReady = true;
				}
				break;
			}
			finally
			{
				reader.Recycle();
			}
		case 207:
			try
			{
				ulong.Parse(reader.ReadString());
				break;
			}
			finally
			{
				reader.Recycle();
			}
		default:
			Debug.Log((object)($"Bad tag {reader.Tag} at {reader.Offset}+{reader.Position}={reader.Length}:  " + string.Join(" ", reader.Buffer.Take(128))));
			reader.Recycle();
			break;
		}
	}

	private IEnumerator CoHandleSpawn(MessageReader reader)
	{
		uint spawnId = reader.ReadPackedUInt32();
		if (spawnId < SpawnableObjects.Length)
		{
			int ownerId = reader.ReadPackedInt32();
			ClientData ownerClient = FindClientById(ownerId);
			int frames = 0;
			while (frames < 1000 && ownerId > 0 && ownerClient == null)
			{
				Debug.LogWarning((object)("Delay spawn for unowned " + spawnId));
				yield return null;
				ownerClient = FindClientById(ownerId);
				int num = frames + 1;
				frames = num;
			}
			if (ownerId > 0 && ownerClient == null)
			{
				yield break;
			}
			InnerNetObject innerNetObject = NonAddressableSpawnableObjects.FirstOrDefault((InnerNetObject f) => f.SpawnId == spawnId);
			InnerNetObject innerNetObject2;
			if (Object.op_Implicit((Object)(object)innerNetObject))
			{
				innerNetObject2 = Object.Instantiate<InnerNetObject>(innerNetObject);
			}
			else
			{
				AsyncOperationHandle<GameObject> spawnPrefab = SpawnableObjects[spawnId].InstantiateAsync((Transform)null, false);
				yield return spawnPrefab;
				innerNetObject2 = spawnPrefab.Result.GetComponent<InnerNetObject>();
			}
			innerNetObject2.SpawnFlags = (SpawnFlags)reader.ReadByte();
			if (innerNetObject2.SpawnFlags.HasFlag(SpawnFlags.IsClientCharacter) && ownerClient != null)
			{
				if (!Object.op_Implicit((Object)(object)ownerClient.Character))
				{
					ownerClient.InScene = true;
					ownerClient.Character = innerNetObject2 as PlayerControl;
					UpdateCachedClients(ownerClient, ownerClient.Character);
				}
				else if (Object.op_Implicit((Object)(object)innerNetObject2))
				{
					Debug.LogWarning((object)$"Double spawn character: {ownerClient.Id} already has {ownerClient.Character.NetId}");
					Object.Destroy((Object)(object)((Component)innerNetObject2).gameObject);
					yield break;
				}
			}
			int num2 = reader.ReadPackedInt32();
			InnerNetObject[] componentsInChildren = ((Component)innerNetObject2).GetComponentsInChildren<InnerNetObject>();
			if (num2 != componentsInChildren.Length)
			{
				Debug.LogError((object)$"Children didn't match for spawnable {spawnId} ({innerNetObject2}): {num2} != {componentsInChildren.Length}");
				Object.Destroy((Object)(object)((Component)innerNetObject2).gameObject);
				yield break;
			}
			for (int num3 = 0; num3 < num2; num3++)
			{
				InnerNetObject innerNetObject3 = componentsInChildren[num3];
				innerNetObject3.NetId = reader.ReadPackedUInt32();
				innerNetObject3.OwnerId = ownerId;
				if (DestroyedObjects.Contains(innerNetObject3.NetId))
				{
					innerNetObject2.NetId = uint.MaxValue;
					Object.Destroy((Object)(object)((Component)innerNetObject2).gameObject);
					break;
				}
				if (!AddNetObject(innerNetObject3))
				{
					innerNetObject2.NetId = uint.MaxValue;
					Object.Destroy((Object)(object)((Component)innerNetObject2).gameObject);
					break;
				}
				MessageReader val = reader.ReadMessage();
				if (val.Length > 0)
				{
					try
					{
						innerNetObject3.Deserialize(val, initialState: true);
					}
					catch (Exception arg)
					{
						Debug.LogError((object)$"Failed to deserialize initial info: {innerNetObject3}: {arg}");
					}
				}
			}
		}
		else
		{
			Debug.LogError((object)("Couldn't find spawnable prefab: " + spawnId));
		}
	}

	public void SetEndpoint(string addr, ushort port, bool dtls)
	{
		networkAddress = addr;
		networkPort = port;
		useDtls = dtls;
	}

	public virtual void Start()
	{
		SceneManager.activeSceneChanged += (Scene oldScene, Scene scene) =>
		{
			SendSceneChange(scene.name);
		};
		ClientId = -1;
		GameId = 32;
	}

	private void SendOrDisconnect(MessageWriter msg)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SendErrors val = ((Connection)connection).Send(msg);
		if ((int)val != 0)
		{
			Debug.Log((object)("Failed to send message: " + ((object)val/*cast due to constrained. prefix*/).ToString()));
			EnqueueDisconnect(DisconnectReasons.Error, "Failed to send message: " + ((object)val/*cast due to constrained. prefix*/).ToString());
		}
	}

	public ClientData GetHost()
	{
		lock (allClients)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if (clientData.Id == HostId)
				{
					return clientData;
				}
			}
		}
		return null;
	}

	public ClientData GetClientFromCharacter(InnerNetObject character)
	{
		if (!Object.op_Implicit((Object)(object)character))
		{
			return null;
		}
		lock (allClients)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if ((Object)(object)clientData.Character == (Object)(object)character)
				{
					return clientData;
				}
			}
		}
		return null;
	}

	public int GetClientIdFromCharacter(InnerNetObject character)
	{
		return GetClientFromCharacter(character)?.Id ?? (-1);
	}

	public virtual void OnDestroy()
	{
		if (AmConnected)
		{
			DisconnectInternal(DisconnectReasons.Destroy);
		}
	}

	public IEnumerator CoConnect(string matchmakerToken)
	{
		if (AmConnected)
		{
			yield break;
		}
		while (true)
		{
			string ipAddr = networkAddress;
			DestroyableSingleton<DisconnectPopup>.Instance.Close();
			LastDisconnectReason = DisconnectReasons.Unknown;
			NetIdCnt = 1u;
			DestroyedObjects.Clear();
			if (DestroyableSingleton<EOSManager>.Instance.ProductUserId == null)
			{
				EnqueueDisconnect(DisconnectReasons.NotAuthorized);
				break;
			}
			if (useDtls && NetworkMode == NetworkModes.OnlineGame)
			{
				connection = (UnityUdpClientConnection)(object)AuthManager.CreateDtlsConnection(ipAddr, (ushort)(networkPort + 3));
			}
			else
			{
				if (NetworkMode == NetworkModes.OnlineGame)
				{
					yield return DestroyableSingleton<AuthManager>.Instance.CoConnect(ipAddr, (ushort)(networkPort + 2), matchmakerToken);
					yield return DestroyableSingleton<AuthManager>.Instance.CoWaitForNonce();
				}
				IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddr), networkPort);
				connection = new UnityUdpClientConnection((ILogger)(object)new UnityLogger(), iPEndPoint, (IPMode)0);
			}
			((UdpConnection)connection).KeepAliveInterval = 1000;
			((UdpConnection)connection).DisconnectTimeoutMs = 7500;
			((UdpConnection)connection).ResendPingMultiplier = 1.2f;
			((Connection)connection).DataReceived += OnMessageReceived;
			((Connection)connection).Disconnected += OnDisconnect;
			lock (Dispatcher)
			{
				Dispatcher.Clear();
			}
			((Connection)connection).ConnectAsync(GetConnectionData(useDtls, matchmakerToken));
			while (connection != null && (int)((Connection)connection).State == 1)
			{
				yield return null;
			}
			if ((connection == null || (int)((Connection)connection).State != 2) && LastDisconnectReason != DisconnectReasons.IncorrectVersion && LastDisconnectReason != DisconnectReasons.InvalidName)
			{
				Debug.Log((object)$"Failed to connected to: {ipAddr}:{networkPort}");
				if (DestroyableSingleton<ServerManager>.Instance.TrackServerFailure(ipAddr))
				{
					DisconnectInternal(DisconnectReasons.NewConnection);
					continue;
				}
				break;
			}
			break;
		}
	}

	private void Connection_DataReceivedRaw(byte[] data)
	{
		Debug.Log((object)("Client Got: " + string.Join(" ", data.Select((byte b) => b.ToString()))));
	}

	private void Connection_DataSentRaw(byte[] data, int length)
	{
		Debug.Log((object)("Client Sent: " + string.Join(" ", data.Select((byte b) => b.ToString()).ToArray(), 0, length)));
	}

	public void Connect(MatchMakerModes mode, string matchmakerToken)
	{
		((MonoBehaviour)this).StartCoroutine(CoConnect(mode, matchmakerToken));
	}

	private IEnumerator CoConnect(MatchMakerModes mode, string matchmakerToken)
	{
		if (this.mode != MatchMakerModes.None)
		{
			DisconnectInternal(DisconnectReasons.NewConnection);
		}
		this.mode = mode;
		isConnecting = true;
		yield return CoConnect(matchmakerToken);
		if (connection == null || (int)((Connection)connection).State != 2)
		{
			HandleDisconnect(DisconnectReasons.Error);
			isConnecting = false;
			yield break;
		}
		switch (this.mode)
		{
		case MatchMakerModes.Client:
			JoinGame();
			yield return WaitWithTimeout(() => ClientId >= 0, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PSNErrorSessionJoinFailed));
			isConnecting = false;
			_ = AmConnected;
			break;
		case MatchMakerModes.HostAndClient:
			GameId = 0;
			GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
			HostGame(GameOptionsManager.Instance.CurrentGameOptions, DataManager.Settings.Multiplayer.HostGameFilterOptions);
			yield return WaitWithTimeout(() => GameId != 0, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorFailedToCreateGame));
			if (!AmConnected)
			{
				isConnecting = false;
				break;
			}
			JoinGame();
			yield return WaitWithTimeout(() => ClientId >= 0, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorFailedToJoinCreatedGame));
			isConnecting = false;
			_ = AmConnected;
			break;
		}
	}

	public IEnumerator WaitForConnectionOrFail()
	{
		while (true)
		{
			if (!AmConnected)
			{
				if (!isConnecting)
				{
					break;
				}
				yield return null;
				continue;
			}
			switch (mode)
			{
			default:
				yield break;
			case MatchMakerModes.Client:
				if (ClientId >= 0)
				{
					yield break;
				}
				break;
			case MatchMakerModes.HostAndClient:
				if (GameId != 0 && ClientId >= 0)
				{
					yield break;
				}
				break;
			}
			yield return null;
		}
	}

	private IEnumerator WaitWithTimeout(Func<bool> success, string errorMessage, int durationSeconds = 15)
	{
		bool failed = true;
		for (float timer = 0f; timer < (float)durationSeconds; timer += Time.deltaTime)
		{
			if (success())
			{
				failed = false;
				break;
			}
			if (!AmConnected)
			{
				yield break;
			}
			yield return null;
		}
		if (failed && errorMessage != null)
		{
			LastCustomDisconnect = errorMessage;
			EnqueueDisconnect(DisconnectReasons.Custom, "Couldn't connect");
		}
	}

	public virtual void Update()
	{
		if (Input.GetKeyDown((KeyCode)13) && (Input.GetKey((KeyCode)308) || Input.GetKey((KeyCode)307)))
		{
			ResolutionManager.ToggleFullscreen();
		}
		TempQueue.Clear();
		lock (Dispatcher)
		{
			TempQueue.AddAll(Dispatcher);
			Dispatcher.Clear();
		}
		for (int i = 0; i < TempQueue.Count; i++)
		{
			Action action = TempQueue[i];
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)"InnerNetClient::Update Exception 1");
				Debug.LogException(ex, (Object)(object)this);
			}
		}
		if (!InOnlineScene)
		{
			return;
		}
		TempQueue.Clear();
		lock (PreSpawnDispatcher)
		{
			TempQueue.AddAll(PreSpawnDispatcher);
			PreSpawnDispatcher.Clear();
		}
		for (int j = 0; j < TempQueue.Count; j++)
		{
			Action action2 = TempQueue[j];
			try
			{
				action2();
			}
			catch (Exception ex2)
			{
				Debug.LogError((object)"InnerNetClient::Update Exception 2");
				Debug.LogException(ex2, (Object)(object)this);
			}
		}
	}

	private void OnDisconnect(object sender, DisconnectedEventArgs e)
	{
		MessageReader message = e.Message;
		if (message != null && message.Position < message.Length)
		{
			if (message.ReadByte() == 1)
			{
				MessageReader val = message.ReadMessage();
				DisconnectReasons disconnectReasons = (DisconnectReasons)val.ReadByte();
				switch (disconnectReasons)
				{
				case DisconnectReasons.Custom:
					LastCustomDisconnect = val.ReadString();
					if (string.IsNullOrWhiteSpace(LastCustomDisconnect))
					{
						LastCustomDisconnect = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorDisconnectPacket);
					}
					break;
				case DisconnectReasons.QuickchatLock:
					LastServerChatMode = val.ReadByte();
					break;
				}
				EnqueueDisconnect(disconnectReasons, LastCustomDisconnect);
			}
			else
			{
				string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorDisconnectPacket);
				LastCustomDisconnect = text + "\r\n\r\n" + (e.Reason ?? "Null");
				EnqueueDisconnect(DisconnectReasons.Custom, LastCustomDisconnect);
			}
		}
		else if (e.Reason == null)
		{
			string text2 = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorDisconnectPacket);
			LastCustomDisconnect = text2 + "\r\n\r\nNull";
			EnqueueDisconnect(DisconnectReasons.Custom, LastCustomDisconnect);
		}
		else if (!e.Reason.Contains("The remote sent a"))
		{
			string text3 = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorDisconnectPacket);
			LastCustomDisconnect = text3 + "\r\n\r\n" + e.Reason;
			EnqueueDisconnect(DisconnectReasons.Custom, LastCustomDisconnect);
		}
		else
		{
			EnqueueDisconnect(DisconnectReasons.Error, e.Reason);
		}
	}

	public void HandleDisconnect(DisconnectReasons reason, string stringReason = null)
	{
		if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
		{
			isConnecting = false;
			((MonoBehaviour)this).StopAllCoroutines();
			DisconnectInternal(reason, stringReason);
			OnDisconnected();
			GameId = -1;
		}
	}

	protected void EnqueueDisconnect(DisconnectReasons reason, string stringReason = null)
	{
		_ = connection;
		lock (Dispatcher)
		{
			Dispatcher.Add(() =>
			{
				HandleDisconnect(reason, stringReason);
			});
		}
	}

	protected void DisconnectInternal(DisconnectReasons reason, string stringReason = null)
	{
		if (!DontBotherLoggingTheseDisconnectReasons.Contains(reason))
		{
			serverLogger.Error(string.Format("Client DC because {0}: {1}", reason, stringReason ?? "null"));
		}
		if (reason != DisconnectReasons.NewConnection && reason != DisconnectReasons.FocusLostBackground)
		{
			LastDisconnectReason = reason;
			if (reason != DisconnectReasons.ExitGame && reason != DisconnectReasons.Unknown && DestroyableSingleton<DisconnectPopup>.InstanceExists)
			{
				DestroyableSingleton<DisconnectPopup>.Instance.Show();
			}
		}
		if (reason != DisconnectReasons.NewConnection)
		{
			if (mode == MatchMakerModes.HostAndClient)
			{
				GameId = 0;
			}
			if (mode == MatchMakerModes.Client || mode == MatchMakerModes.HostAndClient)
			{
				ClientId = -1;
			}
		}
		mode = MatchMakerModes.None;
		GameState = GameStates.NotJoined;
		UnityUdpClientConnection val = Interlocked.Exchange(ref connection, null);
		if (val != null)
		{
			try
			{
				((Connection)val).Dispose();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)"InnerNetClient::DisconnectInternal Exception");
				Debug.LogException(ex, (Object)(object)this);
			}
		}
		if (DestroyableSingleton<InnerNetServer>.InstanceExists)
		{
			DestroyableSingleton<InnerNetServer>.Instance.StopServer();
		}
		lock (Dispatcher)
		{
			Dispatcher.Clear();
		}
		lock (PreSpawnDispatcher)
		{
			PreSpawnDispatcher.Clear();
		}
		if (reason != DisconnectReasons.Error)
		{
			TempQueue.Clear();
		}
		allObjects.Clear();
		allClients.Clear();
		recentClients.Clear();
		allObjectsFast.Clear();
		platformSpecificsChecked = false;
		GameManager.DestroyInstance();
		TempData.OnDisconnected();
	}

	public void HostGame(IGameOptions settings, GameFilterOptions filterOpts)
	{
		IsGamePublic = false;
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)0);
		val.WriteBytesAndSize(gameOptionsFactory.ToBytes(settings));
		val.Write(CrossplayMode.GetCrossplayFlags());
		filterOpts.Serialize(val);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
		Debug.Log((object)"Client requesting new game.");
	}

	public void ReportPlayer(int clientId, ReportReasons reason)
	{
		ClientData recentClient = GetRecentClient(clientId);
		if (recentClient != null)
		{
			recentClient.HasBeenReported = true;
		}
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)17);
		val.Write(GameId);
		val.WritePacked(clientId);
		val.Write((byte)reason);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	public void JoinGame()
	{
		if (NetworkMode == NetworkModes.OnlineGame && DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline)
		{
			LastCustomDisconnect = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PSNErrorUserSignedOut);
			HandleDisconnect(DisconnectReasons.Custom);
			return;
		}
		ClientId = -1;
		if (!AmConnected)
		{
			LastCustomDisconnect = "Disconnected before joining game.";
			HandleDisconnect(DisconnectReasons.Custom);
			return;
		}
		if (Streams == null)
		{
			Streams = new MessageWriter[2];
			for (int i = 0; i < Streams.Length; i++)
			{
				Streams[i] = MessageWriter.Get((SendOption)(byte)i);
			}
		}
		for (int j = 0; j < Streams.Length; j++)
		{
			MessageWriter obj = Streams[j];
			obj.Clear((SendOption)(byte)j);
			obj.StartMessage((byte)5);
			obj.Write(GameId);
		}
		Debug.Log((object)$"Client joining game: {GameId}/{GameCode.IntToGameName(GameId)} useDtls: {useDtls}");
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)1);
		val.Write(GameId);
		val.Write(!DataManager.Settings.Multiplayer.CrossPlay);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	public bool CanBan()
	{
		if (AmHost)
		{
			return !IsGameStarted;
		}
		return false;
	}

	public bool CanKick()
	{
		if (!IsGameStarted)
		{
			return AmHost;
		}
		return true;
	}

	public void KickPlayer(int clientId, bool ban)
	{
		if (AmHost)
		{
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.StartMessage((byte)11);
			val.Write(GameId);
			val.WritePacked(clientId);
			val.Write(ban);
			val.EndMessage();
			SendOrDisconnect(val);
			val.Recycle();
		}
	}

	public MessageWriter StartEndGame()
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)8);
		val.Write(GameId);
		return val;
	}

	public void FinishEndGame(MessageWriter msg)
	{
		msg.EndMessage();
		SendOrDisconnect(msg);
		msg.Recycle();
	}

	protected void SendLateRejection(int targetId, DisconnectReasons reason)
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)4);
		val.Write(GameId);
		val.WritePacked(targetId);
		val.Write((byte)reason);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	protected void SendClientReady()
	{
		ClientData clientData = FindClientById(ClientId);
		if (clientData == null)
		{
			HandleDisconnect(DisconnectReasons.Error, "Couldn't find self");
			return;
		}
		clientData.IsReady = true;
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)5);
		val.Write(GameId);
		val.StartMessage((byte)7);
		val.WritePacked(ClientId);
		val.EndMessage();
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	protected void SendStartGame()
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)2);
		val.Write(GameId);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	public void RequestGameList(IGameOptions settings, GameFilterOptions filterOpts)
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)16);
		val.WritePacked(2);
		val.WriteBytesAndSize(gameOptionsFactory.ToBytes(settings));
		val.Write((byte)DataManager.Settings.Multiplayer.ChatMode);
		val.Write(CrossplayMode.GetCrossplayFlags());
		filterOpts.Serialize(val);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	public void ChangeGamePublic(bool isPublic)
	{
		if (AmHost)
		{
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.StartMessage((byte)10);
			val.Write(GameId);
			val.Write((byte)1);
			val.Write(isPublic);
			val.EndMessage();
			SendOrDisconnect(val);
			val.Recycle();
			IsGamePublic = isPublic;
		}
	}

	public void SetGameSession(string session)
	{
	}

	public void SetActivePodType(string podType)
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)21);
		val.Write(podType ?? "");
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	public void QueryPlatformIds()
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)22);
		val.Write(GameId);
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	private void OnQueryPlatformData(List<PlatformSpecificData> playerData)
	{
		platformSpecificsChecked = true;
		JoinGame();
	}

	private void OnMessageReceived(DataReceivedEventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		MessageReader message = e.Message;
		try
		{
			while (message.Position < message.Length)
			{
				HandleMessage(message.ReadMessage(), e.SendOption);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, (Object)(object)this);
		}
		finally
		{
			message.Recycle();
		}
	}

	private void HandleMessage(MessageReader reader, SendOption sendOption)
	{
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		switch (reader.Tag)
		{
		case 17:
		{
			int clientId = reader.ReadPackedInt32();
			ReportReasons reason = (ReportReasons)reader.ReadInt32();
			ReportOutcome outcome = (ReportOutcome)reader.ReadByte();
			string playerName = reader.ReadString();
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnReportedPlayer(outcome, clientId, playerName, reason);
				});
				break;
			}
		}
		case 0:
			GameId = reader.ReadInt32();
			Debug.Log((object)("Client hosting game: " + GameCode.IntToGameName(GameId)));
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnGameCreated(GameCode.IntToGameName(GameId));
				});
				break;
			}
		case 4:
		{
			int num10 = reader.ReadInt32();
			if (GameId != num10)
			{
				break;
			}
			int playerIdThatLeft = reader.ReadInt32();
			int hostId = reader.ReadInt32();
			DisconnectReasons reason2 = (DisconnectReasons)reader.ReadByte();
			if (!AmHost)
			{
				HostId = hostId;
				if (AmHost)
				{
					lock (Dispatcher)
					{
						Dispatcher.Add(() =>
						{
							OnBecomeHost();
						});
					}
				}
			}
			RemovePlayer(playerIdThatLeft, reason2);
			break;
		}
		case 8:
		{
			int num13 = reader.ReadInt32();
			if (GameId != num13 || GameState == GameStates.Ended)
			{
				break;
			}
			GameState = GameStates.Ended;
			lock (allClients)
			{
				allClients.Clear();
			}
			EndGameResult endGameResult = EndGameResult.Create(reader);
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnGameEnd(endGameResult);
				});
				break;
			}
		}
		case 22:
		{
			int num7 = reader.ReadInt32();
			if (GameId != num7)
			{
				break;
			}
			List<PlatformSpecificData> received = new List<PlatformSpecificData>();
			while (reader.Position < reader.Length)
			{
				PlatformSpecificData platformSpecificData2 = new PlatformSpecificData();
				platformSpecificData2.Deserialize(reader.ReadMessage());
				received.Add(platformSpecificData2);
			}
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnQueryPlatformData(received);
				});
				break;
			}
		}
		case 12:
		{
			int num14 = reader.ReadInt32();
			if (GameId != num14)
			{
				break;
			}
			ClientId = reader.ReadInt32();
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnWaitForHost(GameCode.IntToGameName(GameId));
				});
				break;
			}
		}
		case 7:
		{
			int num3 = reader.ReadInt32();
			if (GameId != num3 || GameState == GameStates.Joined)
			{
				break;
			}
			GameState = GameStates.Joined;
			ClientId = reader.ReadInt32();
			GetOrCreateClient(new ClientData(ClientId, DataManager.Player.Customization.Name, Constants.GetPlatformData(), DataManager.Player.Stats.Level, DestroyableSingleton<EOSManager>.Instance.ProductUserId, DestroyableSingleton<EOSManager>.Instance.FriendCode));
			HostId = reader.ReadInt32();
			int num4 = reader.ReadPackedInt32();
			for (int num5 = 0; num5 < num4; num5++)
			{
				int id = reader.ReadPackedInt32();
				string playerName2 = reader.ReadString();
				PlatformSpecificData platformSpecificData = new PlatformSpecificData();
				platformSpecificData.Deserialize(reader.ReadMessage());
				uint playerLevel = reader.ReadPackedUInt32();
				string productUserId = reader.ReadString();
				string friendCode = reader.ReadString();
				GetOrCreateClient(new ClientData(id, playerName2, platformSpecificData, playerLevel, productUserId, friendCode));
			}
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnGameJoined(GameCode.IntToGameName(GameId));
				});
				break;
			}
		}
		case 1:
		{
			int num11 = reader.ReadInt32();
			if (GameId == num11)
			{
				int num12 = reader.ReadInt32();
				bool amHost = AmHost;
				HostId = reader.ReadInt32();
				string playerName3 = reader.ReadString();
				PlatformSpecificData platformSpecificData3 = new PlatformSpecificData();
				platformSpecificData3.Deserialize(reader.ReadMessage());
				uint playerLevel2 = reader.ReadPackedUInt32();
				string productUserId2 = reader.ReadString();
				string friendCode2 = reader.ReadString();
				ClientData client = GetOrCreateClient(new ClientData(num12, playerName3, platformSpecificData3, playerLevel2, productUserId2, friendCode2));
				Debug.Log((object)$"Player {num12} joined");
				lock (Dispatcher)
				{
					Dispatcher.Add(() =>
					{
						OnPlayerJoined(client);
					});
				}
				if (!AmHost || amHost)
				{
					break;
				}
				lock (Dispatcher)
				{
					Dispatcher.Add(() =>
					{
						OnBecomeHost();
					});
					break;
				}
			}
			EnqueueDisconnect(DisconnectReasons.IncorrectGame);
			break;
		}
		case 5:
		case 6:
		{
			int num8 = reader.ReadInt32();
			if (GameId != num8)
			{
				break;
			}
			if (reader.Tag == 6)
			{
				int num9 = reader.ReadPackedInt32();
				if (ClientId != num9)
				{
					Debug.LogError((object)$"Got data meant for {num9}");
					break;
				}
			}
			if (InOnlineScene)
			{
				MessageReader subReader = MessageReader.Get(reader);
				lock (Dispatcher)
				{
					Dispatcher.Add(() =>
					{
						HandleGameData(subReader);
					});
					break;
				}
			}
			if ((int)sendOption == 0)
			{
				break;
			}
			MessageReader subReader2 = MessageReader.Get(reader);
			lock (PreSpawnDispatcher)
			{
				PreSpawnDispatcher.Add(() =>
				{
					HandleGameData(subReader2);
				});
				break;
			}
		}
		case 16:
		{
			TotalGameData totals = new TotalGameData();
			List<GameListing> output = new List<GameListing>();
			while (reader.Position < reader.Length)
			{
				MessageReader val = reader.ReadMessage();
				switch (val.Tag)
				{
				case 0:
					while (val.Position < val.Length)
					{
						GameListing item = GameListing.DeserializeV2(val.ReadMessage());
						output.Add(item);
					}
					break;
				case 1:
				{
					totals.PerMapTotals = new int[3];
					for (int num6 = 0; num6 < totals.PerMapTotals.Length; num6++)
					{
						totals.PerMapTotals[num6] = val.ReadInt32();
					}
					break;
				}
				}
			}
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					OnGetGameList(totals, output);
				});
				break;
			}
		}
		case 10:
		{
			int num15 = reader.ReadInt32();
			if (GameId == num15)
			{
				if (reader.ReadByte() == 1)
				{
					IsGamePublic = reader.ReadBoolean();
					Debug.Log((object)("Alter Public = " + IsGamePublic));
				}
				else
				{
					Debug.Log((object)"Alter unknown");
				}
			}
			break;
		}
		case 3:
		{
			DisconnectReasons reason3 = DisconnectReasons.ServerRequest;
			if (reader.Position < reader.Length)
			{
				reason3 = (DisconnectReasons)reader.ReadByte();
			}
			EnqueueDisconnect(reason3);
			break;
		}
		case 2:
			GameState = GameStates.Started;
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					((MonoBehaviour)this).StartCoroutine(CoStartGame());
				});
				break;
			}
		case 11:
		{
			int num2 = reader.ReadInt32();
			if (GameId == num2 && reader.ReadPackedInt32() == ClientId)
			{
				bool flag = reader.ReadBoolean();
				if (reader.Position < reader.Length)
				{
					EnqueueDisconnect((DisconnectReasons)reader.ReadByte());
				}
				else
				{
					EnqueueDisconnect(flag ? DisconnectReasons.Banned : DisconnectReasons.Kicked);
				}
			}
			break;
		}
		case 13:
		{
			uint address = reader.ReadUInt32();
			ushort port = reader.ReadUInt16();
			lock (Dispatcher)
			{
				Dispatcher.Add(() =>
				{
					AmongUsClient.Instance.SetEndpoint(AddressToString(address), port, DestroyableSingleton<ServerManager>.Instance.UdpUseDtls);
					Debug.Log((object)$"Redirected to: {networkAddress}:{networkPort} {DestroyableSingleton<ServerManager>.Instance.UdpUseDtls}");
					((MonoBehaviour)this).StopAllCoroutines();
					Connect(mode, null);
				});
				break;
			}
		}
		case byte.MaxValue:
		{
			int num = reader.ReadInt32();
			string arg = reader.ReadString();
			serverLogger.Debug($"v{num}: {arg}");
			break;
		}
		default:
			Debug.Log((object)($"Bad tag {reader.Tag} at {reader.Offset}+{reader.Position}={reader.Length}:  " + string.Join(" ", reader.Buffer.Skip(reader.Offset).Take(reader.Length))));
			break;
		case 14:
		case 20:
			break;
		}
	}

	public static string AddressToString(uint address)
	{
		return $"{(byte)address}.{(byte)(address >> 8)}.{(byte)(address >> 16)}.{(byte)(address >> 24)}";
	}

	private ClientData GetOrCreateClient(ClientData client)
	{
		ClientData clientData;
		lock (allClients)
		{
			clientData = allClients.FirstOrDefault((ClientData c) => c.Id == client.Id);
			if (clientData == null)
			{
				clientData = client;
				allClients.Add(clientData);
			}
		}
		lock (recentClients)
		{
			if (recentClients.FirstOrDefault((ClientData c) => c.Id == client.Id) == null)
			{
				recentClients.Add(clientData);
				recentClients.Sort(RecentPlayerComparer.Instance);
			}
		}
		return clientData;
	}

	public ClientData GetClient(int clientId)
	{
		lock (allClients)
		{
			return allClients.FirstOrDefault((ClientData c) => c.Id == clientId);
		}
	}

	public void GetAllClients(List<ClientData> buffer)
	{
		lock (allClients)
		{
			buffer.Clear();
			buffer.AddRange(allClients);
		}
	}

	public void GetRecentClients(List<ClientData> buffer)
	{
		lock (recentClients)
		{
			buffer.Clear();
			buffer.AddRange(recentClients);
			buffer.Reverse();
		}
	}

	public ClientData GetRecentClient(int clientId)
	{
		lock (recentClients)
		{
			return recentClients.FirstOrDefault((ClientData c) => c.Id == clientId);
		}
	}

	private void RemovePlayer(int playerIdThatLeft, DisconnectReasons reason)
	{
		ClientData client = null;
		lock (allClients)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if (clientData.Id == playerIdThatLeft)
				{
					client = clientData;
					allClients.RemoveAt(i);
					break;
				}
			}
		}
		if (client == null)
		{
			return;
		}
		lock (Dispatcher)
		{
			Dispatcher.Add(() =>
			{
				OnPlayerLeft(client, reason);
			});
		}
	}

	protected virtual void OnApplicationPause(bool pause)
	{
		appPaused = pause;
		if (!pause)
		{
			SecondsSuspendedBeforeDisconnect = 30;
			Debug.Log((object)"Resumed Game");
			if (AmHost)
			{
				RemoveUnownedObjects();
			}
		}
		else if (GameState != GameStates.Ended && AmConnected)
		{
			Debug.Log((object)"Lost focus during game");
			ThreadPool.QueueUserWorkItem(WaitToDisconnect);
		}
	}

	private void WaitToDisconnect(object state)
	{
		for (int i = 0; i < SecondsSuspendedBeforeDisconnect; i++)
		{
			if (!appPaused)
			{
				break;
			}
			Thread.Sleep(1000);
		}
		if (appPaused && GameState != GameStates.Ended && AmConnected)
		{
			DisconnectInternal(DisconnectReasons.FocusLostBackground);
			EnqueueDisconnect(DisconnectReasons.FocusLost);
		}
	}

	protected void SendInitialData(int clientId)
	{
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)6);
		val.Write(GameId);
		val.WritePacked(clientId);
		lock (allObjects)
		{
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			for (int i = 0; i < allObjects.Count; i++)
			{
				InnerNetObject innerNetObject = allObjects[i];
				if (Object.op_Implicit((Object)(object)innerNetObject) && hashSet.Add(((Component)innerNetObject).gameObject))
				{
					WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, val);
				}
			}
		}
		val.EndMessage();
		SendOrDisconnect(val);
		val.Recycle();
	}

	protected abstract void OnGameCreated(string gameIdString);

	protected abstract void OnGameJoined(string gameIdString);

	protected abstract void OnWaitForHost(string gameIdString);

	protected abstract IEnumerator CoStartGame();

	protected abstract void OnGameEnd(EndGameResult endGameResult);

	protected abstract void OnBecomeHost();

	protected abstract void OnPlayerJoined(ClientData client);

	protected abstract IEnumerator CoOnPlayerChangedScene(ClientData client, string targetScene);

	protected abstract void OnPlayerLeft(ClientData client, DisconnectReasons reason);

	protected abstract void OnReportedPlayer(ReportOutcome outcome, int clientId, string playerName, ReportReasons reason);

	protected abstract void OnDisconnected();

	private void OnSetPSNSessionId(string sessionId)
	{
	}

	protected abstract void OnGetGameList(TotalGameData totalGames, List<GameListing> availableGames);

	private byte[] GetConnectionData(bool useDtlsLayout, string matchmakerToken)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		MessageWriter val = new MessageWriter(1000);
		val.Write(Constants.GetBroadcastVersion());
		val.Write(DataManager.Player.Customization.Name);
		if (useDtlsLayout)
		{
			val.Write(matchmakerToken ?? string.Empty);
		}
		else
		{
			val.Write(DestroyableSingleton<AuthManager>.Instance.LastNonceReceived.GetValueOrDefault());
		}
		val.Write((uint)DataManager.Settings.Language.CurrentLanguage);
		val.Write((byte)DataManager.Settings.Multiplayer.ChatMode);
		Constants.GetPlatformData().Serialize(val);
		if (useDtlsLayout)
		{
			val.Write(DestroyableSingleton<EOSManager>.Instance.FriendCode ?? string.Empty);
		}
		else
		{
			val.Write("");
			val.Write(0u);
		}
		return val.ToByteArray(true);
	}

	protected ClientData FindClientById(int id)
	{
		if (id < 0)
		{
			return null;
		}
		lock (allClients)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if (clientData.Id == id)
				{
					return clientData;
				}
			}
			return null;
		}
	}
}
