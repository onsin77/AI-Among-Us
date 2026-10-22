using System;

public class RefreshTimer
{
	public readonly DateTime time;

	public readonly StoreTab[] tabs;

	public RefreshTimer(DateTime time, StoreTab[] tabs)
	{
		this.time = time;
		this.tabs = tabs;
	}
}
