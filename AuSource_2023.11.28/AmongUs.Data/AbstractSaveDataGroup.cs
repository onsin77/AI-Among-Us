using System;

namespace AmongUs.Data;

public abstract class AbstractSaveDataGroup : ISaveDataGroupControl
{
	public event Action OnChangedEvent;

	protected virtual void SetValue<T>(ref T field, T value, Action onChangedCallback)
	{
		if (!field.Equals(value))
		{
			field = value;
			onChangedCallback?.Invoke();
			HandleChange();
		}
	}

	protected virtual void HandleChange()
	{
		OnChangedEvent?.Invoke();
	}

	protected virtual void OnSaveStart()
	{
	}

	protected virtual void OnSaveComplete()
	{
	}

	protected virtual void OnLoadStart()
	{
	}

	protected virtual void OnLoadComplete()
	{
	}

	void ISaveDataGroupControl.SaveStart()
	{
		OnSaveStart();
	}

	void ISaveDataGroupControl.SaveComplete()
	{
		OnSaveComplete();
	}

	void ISaveDataGroupControl.LoadStart()
	{
		OnLoadStart();
	}

	void ISaveDataGroupControl.LoadComplete()
	{
		OnLoadComplete();
	}
}
