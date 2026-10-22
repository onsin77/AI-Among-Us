namespace AmongUs.Data;

public interface ISaveDataGroupControl
{
	void SaveStart();

	void SaveComplete();

	void LoadStart();

	void LoadComplete();
}
