using AmongUs.GameOptions;

public class LogicOptionsNormal : LogicOptions
{
	private NormalGameOptionsV07 GameOptions;

	protected override IGameOptions currentGameOptions => GameOptions;

	public LogicOptionsNormal(GameManager manager)
		: base(manager)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			GameOptions = GameOptionsManager.Instance.CurrentGameOptions as NormalGameOptionsV07;
		}
	}

	public int GetDiscussionTime()
	{
		return GameOptions.DiscussionTime;
	}

	public int GetVotingTime()
	{
		return GameOptions.VotingTime;
	}

	public override int GetEmergencyCooldown()
	{
		return GameOptions.EmergencyCooldown;
	}

	public override int GetNumEmergencyMeetings()
	{
		return GameOptions.NumEmergencyMeetings;
	}

	public override bool GetVisualTasks()
	{
		return GameOptions.VisualTasks;
	}

	public override bool GetAnonymousVotes()
	{
		return GameOptions.AnonymousVotes;
	}

	public override bool GetConfirmImpostor()
	{
		return GameOptions.ConfirmImpostor;
	}

	public override TaskBarMode GetTaskBarMode()
	{
		return (TaskBarMode)GameOptions.TaskBarMode;
	}

	public override void OnGameStart()
	{
	}

	public override void OnGameEnd()
	{
	}

	public override void FixedUpdate()
	{
	}

	public override void OnDestroy()
	{
	}

	protected override void SetGameOptions(IGameOptions newOptions)
	{
		GameOptions = newOptions as NormalGameOptionsV07;
		SetDirty();
	}
}
