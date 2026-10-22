using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Hazel;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet;

public class InnerNetServer : DestroyableSingleton<InnerNetServer>
{
	protected class Player
	{
		private static int IdCount = 1;

		public int Id;

		public Connection Connection;

		public string PlayerName;

		public PlatformSpecificData PlatformData;

		public LimboStates LimboState;

		public Player(Connection connection, string playerName, PlatformSpecificData platformData)
		{
			Id = Interlocked.Increment(ref IdCount);
			Connection = connection;
			PlayerName = playerName;
			PlatformData = platformData;
		}
	}

	public const int LocalGameId = 32;

	private const int InvalidHost = -1;

	private int HostId = -1;

	public PlatformSpecificData PlatformData;

	public HashSet<string> ipBans = new HashSet<string>();

	public int Port = 22023;

	[SerializeField]
	private GameStates GameState;

	private UdpConnectionListener listener;

	private List<Player> Clients = new List<Player>();

	public override void OnDestroy()
	{
		StopServer();
		base.OnDestroy();
	}

	public void StartAsServer()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		if (listener != null)
		{
			StopServer();
		}
		GameState = GameStates.NotStarted;
		listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, Port), (IPMode)0, (ILogger)(object)new UnityLogger());
		((ConnectionListener)listener).NewConnection += OnServerConnect;
		((ConnectionListener)listener).OnInternalError += OnInternalError;
		((ConnectionListener)listener).Start();
	}

	private void OnInternalError(HazelInternalErrors errorType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)errorType == 5)
		{
			AmongUsClient.Instance.ExitGame(DisconnectReasons.FocusLost);
			return;
		}
		Debug.LogError((object)("Internal error in InnerNetServer:" + ((object)errorType/*cast due to constrained. prefix*/).ToString()));
		AmongUsClient.Instance.ExitGame(DisconnectReasons.Error);
	}

	public void StartAsLocalServer()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		if (listener != null)
		{
			StopServer();
		}
		GameState = GameStates.NotStarted;
		listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port), (IPMode)0, (ILogger)(object)new UnityLogger());
		((ConnectionListener)listener).NewConnection += OnServerConnect;
		((ConnectionListener)listener).Start();
	}

	private void DebugString(string obj)
	{
		if (!string.IsNullOrWhiteSpace(obj))
		{
			Debug.LogError((object)obj);
		}
	}

	public void StopServer()
	{
		HostId = -1;
		GameState = GameStates.Destroyed;
		if (listener != null)
		{
			((ConnectionListener)listener).Dispose();
			listener = null;
		}
		lock (Clients)
		{
			Clients.Clear();
		}
	}

	public static bool IsCompatibleVersion(int version)
	{
		return Constants.CompatVersions.Contains(version);
	}

	private void OnServerConnect(NewConnectionEventArgs evt)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		Player client = null;
		MessageReader handshakeData = evt.HandshakeData;
		try
		{
			if (evt.HandshakeData.Length < 5)
			{
				SendIncorrectVersion(evt.Connection);
				return;
			}
			if (!IsCompatibleVersion(handshakeData.ReadInt32()))
			{
				SendIncorrectVersion(evt.Connection);
				return;
			}
			string playerName = handshakeData.ReadString();
			handshakeData.ReadUInt32();
			handshakeData.ReadUInt32();
			handshakeData.ReadByte();
			PlatformSpecificData platformSpecificData = new PlatformSpecificData();
			platformSpecificData.Deserialize(handshakeData);
			CrossplayMode.GetCrossplayFlags();
			client = new Player(evt.Connection, playerName, platformSpecificData);
		}
		finally
		{
			handshakeData.Recycle();
		}
		Debug.Log((object)$"Client {client.Id} added: {evt.Connection.EndPoint}");
		UdpConnection val = (UdpConnection)evt.Connection;
		val.KeepAliveInterval = 1500;
		val.DisconnectTimeoutMs = 6000;
		val.ResendPingMultiplier = 1.5f;
		((Connection)val).DataReceived += (DataReceivedEventArgs e) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			OnDataReceived(client, e);
		};
		((Connection)val).Disconnected += (object o, DisconnectedEventArgs e) =>
		{
			ClientDisconnect(client);
		};
	}

	private static void SendIncorrectVersion(Connection connection)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)1);
		val.Write(5);
		val.EndMessage();
		connection.Send(val);
		val.Recycle();
	}

	private void Connection_DataRecievedRaw(byte[] data, int length)
	{
		Debug.Log((object)("Server Got: " + string.Join(" ", data.Select((byte b) => b.ToString()).Take(length))));
	}

	private void OnDataReceived(Player client, DataReceivedEventArgs evt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		MessageReader message = evt.Message;
		try
		{
			while (message.Position < message.Length)
			{
				HandleMessage(client, message.ReadMessage(), evt.SendOption);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)"InnerNetServer::OnDataReceived Exception");
			Debug.LogException(ex, (Object)(object)this);
		}
		finally
		{
			message.Recycle();
		}
	}

	private void HandleMessage(Player client, MessageReader reader, SendOption sendOption)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		switch (reader.Tag)
		{
		case 1:
		{
			Debug.Log((object)"Server got join game");
			if (reader.ReadInt32() == 32)
			{
				JoinGame(client);
				break;
			}
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.StartMessage((byte)1);
			val.Write((byte)3);
			val.EndMessage();
			client.Connection.Send(val);
			val.Recycle();
			break;
		}
		case 0:
		{
			Debug.Log((object)"Server got host game");
			MessageWriter val3 = MessageWriter.Get((SendOption)1);
			val3.StartMessage((byte)0);
			val3.Write(32);
			val3.EndMessage();
			client.Connection.Send(val3);
			val3.Recycle();
			break;
		}
		case 3:
			if (reader.ReadInt32() == 32)
			{
				ClientDisconnect(client);
			}
			break;
		case 2:
			if (reader.ReadInt32() == 32)
			{
				StartGame(reader, client);
			}
			break;
		case 8:
			if (reader.ReadInt32() == 32)
			{
				EndGame(reader, client);
			}
			break;
		case 6:
			if (Clients.Contains(client))
			{
				if (reader.ReadInt32() == 32)
				{
					int targetId = reader.ReadPackedInt32();
					MessageWriter val4 = MessageWriter.Get(sendOption);
					val4.CopyFrom(reader);
					SendTo(val4, targetId);
					val4.Recycle();
				}
			}
			else if (GameState == GameStates.Started)
			{
				Debug.Log((object)"GameDataTo: Server didn't have client");
				client.Connection.Dispose();
			}
			break;
		case 5:
			if (Clients.Contains(client))
			{
				if (reader.ReadInt32() == 32)
				{
					MessageWriter val2 = MessageWriter.Get(sendOption);
					val2.CopyFrom(reader);
					Broadcast(val2, client);
					val2.Recycle();
				}
			}
			else if (GameState == GameStates.Started)
			{
				Debug.Log((object)"GameDataTo: Server didn't have client");
				client.Connection.Dispose();
			}
			break;
		case 11:
			if (reader.ReadInt32() == 32)
			{
				KickPlayer(reader.ReadPackedInt32(), reader.ReadBoolean());
			}
			break;
		case 4:
		case 7:
		case 9:
		case 10:
			break;
		}
	}

	private void KickPlayer(int targetId, bool ban)
	{
		lock (Clients)
		{
			Player player = null;
			for (int i = 0; i < Clients.Count; i++)
			{
				if (Clients[i].Id == targetId)
				{
					player = Clients[i];
					break;
				}
			}
			if (player == null)
			{
				return;
			}
			if (ban)
			{
				lock (ipBans)
				{
					IPEndPoint endPoint = player.Connection.EndPoint;
					ipBans.Add(endPoint.Address.ToString());
				}
			}
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.StartMessage((byte)11);
			val.Write(32);
			val.WritePacked(targetId);
			val.Write(ban);
			val.EndMessage();
			Broadcast(val, null);
			val.Recycle();
		}
	}

	protected void JoinGame(Player client)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		lock (ipBans)
		{
			IPEndPoint endPoint = client.Connection.EndPoint;
			if (ipBans.Contains(endPoint.Address.ToString()))
			{
				MessageWriter val = MessageWriter.Get((SendOption)1);
				val.StartMessage((byte)1);
				val.Write(6);
				val.EndMessage();
				client.Connection.Send(val);
				val.Recycle();
				return;
			}
		}
		lock (Clients)
		{
			switch (GameState)
			{
			case GameStates.NotStarted:
				HandleNewGameJoin(client);
				return;
			case GameStates.Ended:
				HandleRejoin(client);
				return;
			}
			MessageWriter val2 = MessageWriter.Get((SendOption)1);
			val2.StartMessage((byte)1);
			val2.Write(2);
			val2.EndMessage();
			client.Connection.Send(val2);
			val2.Recycle();
		}
	}

	private void HandleRejoin(Player client)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (client.Id == HostId)
		{
			GameState = GameStates.NotStarted;
			HandleNewGameJoin(client);
			MessageWriter val = MessageWriter.Get((SendOption)1);
			for (int i = 0; i < Clients.Count; i++)
			{
				Player player = Clients[i];
				if (player != client)
				{
					try
					{
						WriteJoinedMessage(player, val, clear: true);
						player.Connection.Send(val);
					}
					catch (Exception ex)
					{
						Debug.LogError((object)("InnerNetServer::HandleRejoin Exception: " + ex.Message));
						Debug.LogException(ex, (Object)(object)this);
					}
				}
			}
			val.Recycle();
			return;
		}
		if (Clients.Count >= 14)
		{
			MessageWriter val2 = MessageWriter.Get((SendOption)1);
			val2.StartMessage((byte)1);
			val2.Write(1);
			val2.EndMessage();
			client.Connection.Send(val2);
			val2.Recycle();
			return;
		}
		Clients.Add(client);
		client.LimboState = LimboStates.WaitingForHost;
		MessageWriter val3 = MessageWriter.Get((SendOption)1);
		try
		{
			val3.StartMessage((byte)12);
			val3.Write(32);
			val3.Write(client.Id);
			val3.EndMessage();
			client.Connection.Send(val3);
			BroadcastJoinMessage(client, val3);
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("InnerNetServer::HandleRejoin MessageWriter Exception: " + ex2.Message));
			Debug.LogException(ex2, (Object)(object)this);
		}
		finally
		{
			val3.Recycle();
		}
	}

	private void HandleNewGameJoin(Player client)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (Clients.Count >= 15)
		{
			MessageWriter val = MessageWriter.Get((SendOption)1);
			try
			{
				val.StartMessage((byte)1);
				val.Write(1);
				val.EndMessage();
				client.Connection.Send(val);
				return;
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("InnerNetServer::HandleNewGameJoin MessageWriter 1 Exception: " + ex.Message));
				Debug.LogException(ex, (Object)(object)this);
				return;
			}
			finally
			{
				val.Recycle();
			}
		}
		Clients.Add(client);
		client.LimboState = LimboStates.PreSpawn;
		if (HostId == -1)
		{
			HostId = Clients[0].Id;
		}
		if (HostId == client.Id)
		{
			client.LimboState = LimboStates.NotLimbo;
		}
		MessageWriter val2 = MessageWriter.Get((SendOption)1);
		try
		{
			WriteJoinedMessage(client, val2, clear: true);
			client.Connection.Send(val2);
			BroadcastJoinMessage(client, val2);
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("InnerNetServer::HandleNewGameJoin MessageWriter 2 Exception: " + ex2.Message));
			Debug.LogException(ex2, (Object)(object)this);
		}
		finally
		{
			val2.Recycle();
		}
	}

	private void EndGame(MessageReader message, Player source)
	{
		if (source.Id == HostId)
		{
			GameState = GameStates.Ended;
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.CopyFrom(message);
			Broadcast(val, null);
			val.Recycle();
			lock (Clients)
			{
				Clients.Clear();
				return;
			}
		}
		Debug.LogWarning((object)("Reset request rejected from: " + source.Id));
	}

	private void StartGame(MessageReader message, Player source)
	{
		GameState = GameStates.Started;
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.CopyFrom(message);
		Broadcast(val, null);
		val.Recycle();
	}

	private void ClientDisconnect(Player client)
	{
		Debug.Log((object)("Server DC client " + client.Id));
		MessageWriter val = MessageWriter.Get((SendOption)1);
		val.StartMessage((byte)4);
		val.Write(32);
		val.Write(client.Id);
		val.Write(HostId);
		val.Write((byte)0);
		val.EndMessage();
		Broadcast(val, null);
		val.Recycle();
		lock (Clients)
		{
			Clients.Remove(client);
			client.Connection.Dispose();
			if (Clients.Count > 0)
			{
				HostId = Clients[0].Id;
			}
		}
	}

	protected void SendTo(MessageWriter msg, int targetId)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		lock (Clients)
		{
			for (int i = 0; i < Clients.Count; i++)
			{
				Player player = Clients[i];
				if (player.Id == targetId)
				{
					try
					{
						player.Connection.Send(msg);
						break;
					}
					catch (Exception ex)
					{
						Debug.LogError((object)"InnerNetServer::SendTo Exception");
						Debug.LogException(ex, (Object)(object)this);
						break;
					}
				}
			}
		}
	}

	protected void Broadcast(MessageWriter msg, Player source)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		lock (Clients)
		{
			for (int i = 0; i < Clients.Count; i++)
			{
				Player player = Clients[i];
				if (player != source)
				{
					player.Connection.Send(msg);
				}
			}
		}
	}

	private void BroadcastJoinMessage(Player client, MessageWriter msg)
	{
		msg.Clear((SendOption)1);
		msg.StartMessage((byte)1);
		msg.Write(32);
		msg.Write(client.Id);
		msg.Write(HostId);
		msg.Write(client.PlayerName);
		client.PlatformData.Serialize(msg);
		msg.WritePacked(0);
		msg.Write(string.Empty);
		msg.Write(string.Empty);
		msg.EndMessage();
		Broadcast(msg, client);
	}

	private void WriteJoinedMessage(Player client, MessageWriter msg, bool clear)
	{
		if (clear)
		{
			msg.Clear((SendOption)1);
		}
		msg.StartMessage((byte)7);
		msg.Write(32);
		msg.Write(client.Id);
		msg.Write(HostId);
		msg.WritePacked(Clients.Count - 1);
		for (int i = 0; i < Clients.Count; i++)
		{
			Player player = Clients[i];
			if (player != client)
			{
				msg.WritePacked(player.Id);
				msg.Write(player.PlayerName);
				player.PlatformData.Serialize(msg);
				msg.WritePacked(0);
				msg.Write(string.Empty);
				msg.Write(string.Empty);
			}
		}
		msg.EndMessage();
	}
}
