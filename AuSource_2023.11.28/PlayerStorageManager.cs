using System;
using System.Collections;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.PlayerDataStorage;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerStorageManager : DestroyableSingleton<PlayerStorageManager>
{
	public struct CloudPlayerPrefs
	{
		public string ActivePodType;

		public string receiptRedemptionAccounts;

		public uint PlatformsLimitedItemsValidated;

		public uint IsAdult;
	}

	private const int MaxEOSChunkLength = 40960;

	private const string PlayerPrefsFilename = "player.prefs";

	private PlayerDataStorageInterface playerStorage;

	public Coroutine saveRoutine;

	public CloudPlayerPrefs PlayerPrefs { get; private set; }

	public UpdateState PlayerStorageLoadState { get; private set; }

	public void RefreshAll(PlatformInterface platformInterface)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected Obj, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected Obj, but got Unknown
		if ((Handle)(object)platformInterface == (Handle)null)
		{
			Debug.LogError((object)"Player Storage: Platform Interface was null.");
			return;
		}
		playerStorage = platformInterface.GetPlayerDataStorageInterface();
		if ((Handle)(object)playerStorage == (Handle)null)
		{
			Debug.LogError((object)"Player storage null, is PlatformInterface.Create is misconfigured?");
			return;
		}
		ReadFileOptions val = new ReadFileOptions();
		val.ReadChunkLengthBytes = 40960u;
		val.Filename = "player.prefs";
		val.LocalUserId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		MemoryStream memoryStream = new MemoryStream();
		val.ReadFileDataCallback = OnReadChunkFromEOS;
		playerStorage.ReadFile(val, (object)memoryStream, (OnReadFileCompleteCallback)OnReadPlayerPrefsComplete);
	}

	public void SaveChanges(CloudPlayerPrefs newPrefs)
	{
		PlayerPrefs = newPrefs;
		if (saveRoutine == null)
		{
			saveRoutine = ((MonoBehaviour)this).StartCoroutine(CoSaveChanges());
		}
	}

	private IEnumerator CoSaveChanges()
	{
		yield return (object)new WaitForSeconds(5f);
		saveRoutine = null;
		WriteFileOptions val = new WriteFileOptions();
		val.ChunkLengthBytes = 40960u;
		val.Filename = "player.prefs";
		val.LocalUserId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		val.WriteFileDataCallback = OnWriteChunkToEOS;
		PlayerDataStorageInterface val2 = playerStorage;
		if (val2 != null)
		{
			val2.WriteFile(val, (object)ObjectToStream(PlayerPrefs), (OnWriteFileCompleteCallback)OnWriteFileComplete);
		}
	}

	private WriteResult OnWriteChunkToEOS(WriteFileDataCallbackInfo data, out byte[] outDataBuffer)
	{
		Stream stream = (Stream)data.ClientData;
		long num = Math.Min(stream.Length - stream.Position, 40960L);
		outDataBuffer = new byte[num];
		if (outDataBuffer.Length != 0)
		{
			stream.Read(outDataBuffer, 0, outDataBuffer.Length);
		}
		if (stream.Position < stream.Length)
		{
			return (WriteResult)1;
		}
		return (WriteResult)2;
	}

	private void OnWriteFileComplete(WriteFileCallbackInfo data)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		using ((MemoryStream)data.ClientData)
		{
			if ((int)data.ResultCode != 0)
			{
				Debug.LogError((object)$"Failed to store EOS file {data.Filename}: {data.ResultCode}");
			}
		}
	}

	private ReadResult OnReadChunkFromEOS(ReadFileDataCallbackInfo data)
	{
		((Stream)data.ClientData).Write(data.DataChunk, 0, data.DataChunk.Length);
		return (ReadResult)1;
	}

	private void OnReadPlayerPrefsComplete(ReadFileCallbackInfo data)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		using MemoryStream stream = (MemoryStream)data.ClientData;
		OnReadPlayerPrefsCompleteInternal(stream, data.ResultCode);
	}

	private void OnReadPlayerPrefsCompleteInternal(MemoryStream stream, Result resultCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)resultCode != 0)
		{
			Debug.LogWarning((object)("Didn't read EOS player prefs: " + ((object)resultCode/*cast due to constrained. prefix*/).ToString()));
			PlayerStorageLoadState = UpdateState.Success;
			return;
		}
		stream.Position = 0L;
		using StreamReader streamReader = new StreamReader(stream);
		string text = streamReader.ReadToEnd();
		try
		{
			PlayerPrefs = JsonConvert.DeserializeObject<CloudPlayerPrefs>(text);
		}
		catch
		{
			Debug.LogWarning((object)("Got invalid data from EOS player prefs: " + text));
			PlayerStorageLoadState = UpdateState.Failed;
			return;
		}
		PlayerStorageLoadState = UpdateState.Success;
	}

	public IEnumerator WaitForLoad()
	{
		while (PlayerStorageLoadState == UpdateState.Connecting)
		{
			yield return null;
		}
	}

	private static MemoryStream ObjectToStream(object dataToStream)
	{
		string value = JsonConvert.SerializeObject(dataToStream);
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(value);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}
}
