using System;
using System.Text;
using Innersloth.IO;
using Newtonsoft.Json;

namespace AmongUs.Data;

public abstract class AbstractSaveData : AbstractSaveDataGroup
{
	[JsonProperty("dataVersion")]
	protected int dataVersion;

	private const byte XOR_KEY = 212;

	public bool IsDirty { get; private set; }

	protected abstract AbstractSaveDataGroup[] Groups { get; }

	protected virtual int CurrentVersion => 0;

	public event Action OnSavedEvent;

	public event Action OnLoadedEvent;

	public AbstractSaveData()
	{
		if (Groups != null)
		{
			AbstractSaveDataGroup[] groups = Groups;
			for (int i = 0; i < groups.Length; i++)
			{
				groups[i].OnChangedEvent += HandleChange;
			}
		}
	}

	protected abstract void HandleSave();

	protected abstract void HandleLoad();

	protected abstract void HandleDelete();

	protected abstract bool HandleDoesFileExist();

	protected override void HandleChange()
	{
		IsDirty = true;
		base.HandleChange();
	}

	public bool DoesFileExist()
	{
		return HandleDoesFileExist();
	}

	public void Save()
	{
		if (IsDirty)
		{
			ForceSave();
		}
	}

	public void ForceSave()
	{
		OnSaveStart();
		ForEachGroup((ISaveDataGroupControl group) =>
		{
			group.SaveStart();
		});
		HandleSave();
		OnSaveComplete();
		ForEachGroup((ISaveDataGroupControl group) =>
		{
			group.SaveComplete();
		});
		IsDirty = false;
		OnSavedEvent?.Invoke();
	}

	public void Load()
	{
		if (IsDirty)
		{
			ForceLoad();
		}
	}

	public void ForceLoad()
	{
		OnLoadStart();
		ForEachGroup((ISaveDataGroupControl group) =>
		{
			group.LoadStart();
		});
		HandleLoad();
		OnLoadComplete();
		ForEachGroup((ISaveDataGroupControl group) =>
		{
			group.LoadComplete();
		});
		CheckVersionMigration();
		IsDirty = false;
		OnLoadedEvent?.Invoke();
	}

	private void ForEachGroup(Action<ISaveDataGroupControl> action)
	{
		Groups?.ForEach(action);
	}

	public void Delete()
	{
		HandleDelete();
	}

	private void CheckVersionMigration()
	{
		if (dataVersion < CurrentVersion)
		{
			HandleVersionMigration(dataVersion);
		}
	}

	protected virtual void HandleVersionMigration(int version)
	{
	}

	protected static bool TrySaveToJsonFile(object value, string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			DataManager.Logger.Error("[AbstractSaveData::TrySaveToJsonFile()] Filename cannot be empty. Aborting.");
			return false;
		}
		if (value == null)
		{
			DataManager.Logger.Error("[AbstractSaveData::TrySaveToJsonFile()] Value cannot be null. Aborting.");
			return false;
		}
		string contents = JsonConvert.SerializeObject(value, (Formatting)1);
		FileIO.WriteAllText(FileIO.GetDataPathTo(filename), contents);
		return true;
	}

	protected static bool TryLoadFromJsonFile<T>(string filename, out T output)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			DataManager.Logger.Error("[AbstractSaveData::TryLoadFromJsonFile()] Filename cannot be empty. Aborting.");
			output = default;
			return false;
		}
		string dataPathTo = FileIO.GetDataPathTo(filename);
		if (!FileIO.Exists(dataPathTo))
		{
			DataManager.Logger.Info("[AbstractSaveData::TryLoadFromJsonFile()] File at path '" + dataPathTo + "' does not exist. Creating and returning new object instead.");
			output = default;
			return false;
		}
		string text = FileIO.ReadAllText(dataPathTo);
		if (string.IsNullOrWhiteSpace(text))
		{
			DataManager.Logger.Info("[AbstractSaveData::TryLoadFromJsonFile()] JSON data was empty. Creating and returning new object instead.");
			output = default;
			return false;
		}
		output = JsonConvert.DeserializeObject<T>(text);
		DataManager.Logger.Info("[AbstractSaveData::TryLoadFromJsonFile()] Successfully loaded " + typeof(T).Name + " data from JSON file!");
		return true;
	}

	protected static bool TryLoadAndOverwriteFromJsonFile<T>(T target, string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			DataManager.Logger.Error("[AbstractSaveData::TryLoadAndOverwriteFromJsonFile()] Filename cannot be empty. Aborting.");
			return false;
		}
		if (target == null)
		{
			DataManager.Logger.Error("[AbstractSaveData::TryLoadAndOverwriteFromJsonFile()] Overwrite target cannot be null. Aborting.");
			return false;
		}
		string dataPathTo = FileIO.GetDataPathTo(filename);
		if (!FileIO.Exists(dataPathTo))
		{
			DataManager.Logger.Error("[AbstractSaveData::TryLoadAndOverwriteFromJsonFile()] File at path '" + dataPathTo + "' does not exist. Failed to overwrite target object.");
			return false;
		}
		string text = FileIO.ReadAllText(dataPathTo);
		if (string.IsNullOrWhiteSpace(text))
		{
			DataManager.Logger.Error("[AbstractSaveData::TryLoadAndOverwriteFromJsonFile()] JSON data was empty. Failed to overwrite target object.");
			return false;
		}
		JsonConvert.PopulateObject(text, (object)target);
		DataManager.Logger.Info("[AbstractSaveData::TryLoadAndOverwriteFromJsonFile()] Successfully loaded data from JSON file '" + filename + "'");
		return true;
	}

	protected bool TrySerializeAndEncrypt(object target, out byte[] output)
	{
		if (target == null)
		{
			DataManager.Logger.Error("[AbstractSaveData::TrySerializeAndEncrypt()] Input target was null. Aborting.");
			output = new byte[0];
			return false;
		}
		string text = "";
		try
		{
			text = JsonConvert.SerializeObject(target, (Formatting)1);
		}
		catch (Exception ex)
		{
			DataManager.Logger.Error("[AbstractSaveData::TrySerializeAndEncrypt()] Could not serialize target. Aborting.\r\nException: " + ex.GetType().Name + "\r\n" + ex.Message);
			output = new byte[0];
			return false;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] ^= (byte)(i % 212);
		}
		DataManager.Logger.Debug(string.Format("[{0}::{1}()] Encrypted {2} string successfully.\r\r\n\nEncrypted bytes:\r\n{3}\r\r\n\nDecrypted string:\r\n{4}\r\n", "AbstractSaveData", "TrySerializeAndEncrypt", target.GetType().Name, bytes, text));
		output = bytes;
		return true;
	}

	protected bool TryDecryptAndDeserialize<T>(ref T target, byte[] encryptedBytes)
	{
		if (encryptedBytes == null || encryptedBytes.Length < 1)
		{
			DataManager.Logger.Error("[AbstractSaveData::TryDecryptAndDeserialize()] Supplied encryptedBytes array is null or empty. Aborting.");
			return false;
		}
		byte[] array = new byte[encryptedBytes.Length];
		for (int i = 0; i < encryptedBytes.Length; i++)
		{
			array[i] = encryptedBytes[i];
			array[i] ^= (byte)(i % 212);
		}
		string text = Encoding.UTF8.GetString(array);
		if (string.IsNullOrWhiteSpace(text))
		{
			DataManager.Logger.Error("[AbstractSaveData::TryDecryptAndDeserialize()] Could not decode UTF8 string from bytes. Aborting.");
			return false;
		}
		try
		{
			JsonConvert.PopulateObject(text, (object)target);
		}
		catch (Exception ex)
		{
			DataManager.Logger.Error(string.Format("[{0}::{1}()] Decrypted string unsuccessfully. This should not be happening!\r\r\n\nEncrypted bytes:\r\n{2}\r\r\n\nDecrypted string:\r\n{3}\r\nException: {4}\r\n{5}", "AbstractSaveData", "TryDecryptAndDeserialize", encryptedBytes, text, ex.GetType().Name, ex.Message));
			return false;
		}
		DataManager.Logger.Debug(string.Format("[{0}::{1}()] Decrypted {2} string successfully.\r\r\n\nEncrypted bytes:\r\n{3}\r\r\n\nDecrypted string:\r\n{4}\r\n", "AbstractSaveData", "TryDecryptAndDeserialize", typeof(T).Name, encryptedBytes, text));
		return true;
	}
}
