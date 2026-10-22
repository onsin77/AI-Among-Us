public class LogicMinigameHnS : LogicMinigame
{
	private HideAndSeekManager HideAndSeekManager;

	public LogicMinigameHnS(HideAndSeekManager manager)
		: base(manager)
	{
		HideAndSeekManager = manager;
	}

	public override void OnMinigameOpen()
	{
		base.OnMinigameOpen();
		HideAndSeekManager.LogicMusic.SetTaskState(isDoingTask: true);
	}

	public override void OnMinigameClose()
	{
		base.OnMinigameClose();
		HideAndSeekManager.LogicMusic.SetTaskState(isDoingTask: false);
	}
}
