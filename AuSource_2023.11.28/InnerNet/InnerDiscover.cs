using System;
using System.Collections;
using System.Net.Sockets;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet;

public class InnerDiscover : DestroyableSingleton<InnerDiscover>
{
	private UdpBroadcastListener listener;

	private UdpBroadcaster sender;

	public int Port = 47777;

	public float Interval = 1f;

	public event Action<BroadcastPacket> OnPacketGet;

	public void StartAsServer(string data)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected Obj, but got Unknown
		bool flag = sender == null;
		if (flag)
		{
			sender = new UdpBroadcaster(Port, (Action<string>)Debug.LogError);
		}
		sender.SetData(data);
		if (flag)
		{
			((MonoBehaviour)this).StartCoroutine(RunServer());
		}
	}

	private IEnumerator RunServer()
	{
		while (sender != null)
		{
			sender.Broadcast();
			for (float timer = 0f; timer < Interval; timer += Time.deltaTime)
			{
				yield return null;
			}
		}
	}

	public void StopServer()
	{
		if (sender != null)
		{
			sender.Dispose();
			sender = null;
		}
	}

	public void StartAsClient()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected Obj, but got Unknown
		if (listener == null)
		{
			try
			{
				listener = new UdpBroadcastListener(Port, (Action<string>)Debug.LogError);
				listener.StartListen();
				((MonoBehaviour)this).StartCoroutine(RunClient());
			}
			catch (SocketException ex)
			{
				Debug.LogError((object)"InnerDiscover::StartAsClient SocketException");
				Debug.LogException((Exception)ex, (Object)(object)this);
				AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
				AmongUsClient.Instance.LastCustomDisconnect = "Couldn't start local network listener. You may need to restart Among Us.";
				DestroyableSingleton<DisconnectPopup>.Instance.Show();
			}
		}
	}

	private IEnumerator RunClient()
	{
		while (listener != null)
		{
			listener.StartListen();
			BroadcastPacket[] packets = listener.GetPackets();
			for (int i = 0; i < packets.Length; i++)
			{
				OnPacketGet?.Invoke(packets[i]);
			}
			yield return null;
		}
	}

	public void StopClient()
	{
		if (listener != null)
		{
			listener.Dispose();
			listener = null;
		}
	}

	public override void OnDestroy()
	{
		StopServer();
		StopClient();
		base.OnDestroy();
	}
}
