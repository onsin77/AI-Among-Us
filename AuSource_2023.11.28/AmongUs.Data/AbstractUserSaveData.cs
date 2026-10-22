using Innersloth.IO;

namespace AmongUs.Data;

public abstract class AbstractUserSaveData : AbstractSaveData
{
	protected abstract string FileName { get; }

	protected override void HandleSave()
	{
		if (!AbstractSaveData.TrySaveToJsonFile(this, GetFileName()))
		{
			DataManager.Logger.Error("[" + GetType().Name + "] Failed to save.");
		}
	}

	protected override void HandleLoad()
	{
		if (!DoesFileExist())
		{
			DataManager.Logger.Info("[" + GetType().Name + "] File '" + FileName + "' does not exist. Using default value.");
		}
		else if (!AbstractSaveData.TryLoadAndOverwriteFromJsonFile(this, GetFileName()))
		{
			DataManager.Logger.Error("[" + GetType().Name + "] Loading file '" + FileName + "' failed.");
		}
	}

	protected override void HandleDelete()
	{
		if (DoesFileExist())
		{
			FileIO.Delete(FileIO.GetDataPathTo(GetFileName()));
		}
	}

	protected override bool HandleDoesFileExist()
	{
		return FileIO.Exists(FileIO.GetDataPathTo(GetFileName()));
	}

	private string GetFileName()
	{
		return FileName;
	}

	private bool ShouldIncludeUserIdInPath()
	{
		return false;
	}
}
