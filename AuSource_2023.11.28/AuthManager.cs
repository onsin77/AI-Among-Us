using System.Collections;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Hazel;
using Hazel.Dtls;
using Hazel.Udp;
using InnerNet;
using UnityEngine;

public class AuthManager : DestroyableSingleton<AuthManager>
{
	private const string ServerCertification = "\r\n-----BEGIN CERTIFICATE-----\r\nMIIDbTCCAlWgAwIBAgIUf8xD1G/d5NK1MTjQAYGqd1AmBvcwDQYJKoZIhvcNAQEL\r\nBQAwRTELMAkGA1UEBhMCQVUxEzARBgNVBAgMClNvbWUtU3RhdGUxITAfBgNVBAoM\r\nGEludGVybmV0IFdpZGdpdHMgUHR5IEx0ZDAgFw0yMTAyMDIxNzE4MDFaGA8yMjk0\r\nMTExODE3MTgwMVowRTELMAkGA1UEBhMCQVUxEzARBgNVBAgMClNvbWUtU3RhdGUx\r\nITAfBgNVBAoMGEludGVybmV0IFdpZGdpdHMgUHR5IEx0ZDCCASIwDQYJKoZIhvcN\r\nAQEBBQADggEPADCCAQoCggEBAL7GFDbZdXwPYXeHWRi2GfAXkaLCgxuSADfa1pI2\r\nvJkvgMTK1miSt3jNSg/o6VsjSOSL461nYmGCF6Ho3fMhnefOhKaaWu0VxF0GR1bd\r\ne836YWzhWINQRwmoVD/Wx1NUjLRlTa8g/W3eE5NZFkWI70VOPRJpR9SqjNHwtPbm\r\nKi41PVgJIc3m/7cKOEMrMYNYoc6E9ehwLdJLQ5olJXnMoGjHo2d59hC8KW2V1dY9\r\nsacNPUjbFZRWeQ0eJ7kbn8m3a5EuF34VEC7DFcP4NCWWI7HO5/KYE+mUNn0qxgua\r\nr32qFnoaKZr9dXWRWJSm2XecBgqQmeF/90gdbohNNHGC/iMCAwEAAaNTMFEwHQYD\r\nVR0OBBYEFAJAdUS5AZE3U3SPQoG06Ahq3wBbMB8GA1UdIwQYMBaAFAJAdUS5AZE3\r\nU3SPQoG06Ahq3wBbMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEB\r\nALUoaAEuJf4kQ1bYVA2ax2QipkUM8PL9zoNiDjUw6ZlwMFi++XCQm8XDap45aaeZ\r\nMnXGBqIBWElezoH6BNSbdGwci/ZhxXHG/qdHm7zfCTNaLBe2+sZkGic1x6bZPFtK\r\nZUjGy7LmxsXOxqGMgPhAV4JbN1+LTmOkOutfHiXKe4Z1zu09mOo9sWfGCkbIyERX\r\nQQILBYSIkg3hU4R4xMOjvxcDrOZja6fSNyi2sgidTfe5OCKC2ovU7OmsQqzb7mFv\r\ne+7kpIUp6AZNc49n6GWtGeOoL7JUAqMOIO+R++YQN7/dgaGDPuu0PpmgI2gPLNW1\r\nZwHJ755zQQRX528xg9vfykY=\r\n-----END CERTIFICATE-----\r\n";

	private const byte AuthTagNonceMessage = 1;

	public DtlsUnityConnection connection;

	public uint? LastNonceReceived;

	public IEnumerator CoConnect(string targetIp, ushort targetPort, string matchmakerToken)
	{
		if (connection != null)
		{
			((Connection)connection).DataReceived -= Connection_DataReceived;
			((Connection)connection).Disconnected -= Connection_Disconnected;
			((Connection)connection).Dispose();
			connection = null;
		}
		connection = CreateDtlsConnection(targetIp, targetPort);
		((Connection)connection).DataReceived += Connection_DataReceived;
		((Connection)connection).Disconnected += Connection_Disconnected;
		((Connection)connection).ConnectAsync(BuildData(matchmakerToken));
		while (connection != null && (int)((Connection)connection).State == 1)
		{
			yield return null;
		}
	}

	public static DtlsUnityConnection CreateDtlsConnection(string targetIp, ushort targetPort)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		IPAddress address = IPAddress.Parse(targetIp);
		DtlsUnityConnection val = new DtlsUnityConnection((ILogger)(object)new UnityLogger(), new IPEndPoint(address, targetPort), (IPMode)0);
		X509Certificate2 certificate = new X509Certificate2(CryptoHelpers.DecodePEM("\r\n-----BEGIN CERTIFICATE-----\r\nMIIDbTCCAlWgAwIBAgIUf8xD1G/d5NK1MTjQAYGqd1AmBvcwDQYJKoZIhvcNAQEL\r\nBQAwRTELMAkGA1UEBhMCQVUxEzARBgNVBAgMClNvbWUtU3RhdGUxITAfBgNVBAoM\r\nGEludGVybmV0IFdpZGdpdHMgUHR5IEx0ZDAgFw0yMTAyMDIxNzE4MDFaGA8yMjk0\r\nMTExODE3MTgwMVowRTELMAkGA1UEBhMCQVUxEzARBgNVBAgMClNvbWUtU3RhdGUx\r\nITAfBgNVBAoMGEludGVybmV0IFdpZGdpdHMgUHR5IEx0ZDCCASIwDQYJKoZIhvcN\r\nAQEBBQADggEPADCCAQoCggEBAL7GFDbZdXwPYXeHWRi2GfAXkaLCgxuSADfa1pI2\r\nvJkvgMTK1miSt3jNSg/o6VsjSOSL461nYmGCF6Ho3fMhnefOhKaaWu0VxF0GR1bd\r\ne836YWzhWINQRwmoVD/Wx1NUjLRlTa8g/W3eE5NZFkWI70VOPRJpR9SqjNHwtPbm\r\nKi41PVgJIc3m/7cKOEMrMYNYoc6E9ehwLdJLQ5olJXnMoGjHo2d59hC8KW2V1dY9\r\nsacNPUjbFZRWeQ0eJ7kbn8m3a5EuF34VEC7DFcP4NCWWI7HO5/KYE+mUNn0qxgua\r\nr32qFnoaKZr9dXWRWJSm2XecBgqQmeF/90gdbohNNHGC/iMCAwEAAaNTMFEwHQYD\r\nVR0OBBYEFAJAdUS5AZE3U3SPQoG06Ahq3wBbMB8GA1UdIwQYMBaAFAJAdUS5AZE3\r\nU3SPQoG06Ahq3wBbMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEB\r\nALUoaAEuJf4kQ1bYVA2ax2QipkUM8PL9zoNiDjUw6ZlwMFi++XCQm8XDap45aaeZ\r\nMnXGBqIBWElezoH6BNSbdGwci/ZhxXHG/qdHm7zfCTNaLBe2+sZkGic1x6bZPFtK\r\nZUjGy7LmxsXOxqGMgPhAV4JbN1+LTmOkOutfHiXKe4Z1zu09mOo9sWfGCkbIyERX\r\nQQILBYSIkg3hU4R4xMOjvxcDrOZja6fSNyi2sgidTfe5OCKC2ovU7OmsQqzb7mFv\r\ne+7kpIUp6AZNc49n6GWtGeOoL7JUAqMOIO+R++YQN7/dgaGDPuu0PpmgI2gPLNW1\r\nZwHJ755zQQRX528xg9vfykY=\r\n-----END CERTIFICATE-----\r\n"));
		val.SetValidServerCertificates(new X509Certificate2Collection { certificate });
		return val;
	}

	public IEnumerator CoWaitForNonce(float timeout = 5f)
	{
		for (float timer = 0f; timer < timeout; timer += Time.deltaTime)
		{
			if (connection == null)
			{
				break;
			}
			if ((int)((Connection)connection).State != 2)
			{
				break;
			}
			if (LastNonceReceived.HasValue)
			{
				break;
			}
			yield return null;
		}
	}

	private void FixedUpdate()
	{
		DtlsUnityConnection val = connection;
		if (val != null)
		{
			((UnityUdpClientConnection)val).FixedUpdate();
		}
	}

	public override void OnDestroy()
	{
		if (connection != null)
		{
			((Connection)connection).Dispose();
			connection = null;
		}
		base.OnDestroy();
	}

	private void Connection_Disconnected(object sender, DisconnectedEventArgs e)
	{
		((Connection)connection).Dispose();
		connection = null;
	}

	private void Connection_DataReceived(DataReceivedEventArgs obj)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MessageReader message = obj.Message;
		try
		{
			MessageReader val = message.ReadMessage();
			if (val.Tag == 1)
			{
				LastNonceReceived = val.ReadUInt32();
				((Connection)connection).Disconnect("Job done", (MessageWriter)null);
			}
		}
		finally
		{
			message.Recycle();
		}
	}

	private byte[] BuildData(string matchmakerToken)
	{
		MessageWriter val = MessageWriter.Get((SendOption)0);
		val.Write(Constants.GetBroadcastVersion());
		val.Write((byte)Constants.GetPlatformType());
		val.Write(matchmakerToken ?? string.Empty);
		val.Write(DestroyableSingleton<EOSManager>.Instance.FriendCode ?? string.Empty);
		return val.ToByteArray(false);
	}
}
