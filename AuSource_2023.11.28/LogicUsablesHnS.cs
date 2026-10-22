using Hazel;

public class LogicUsablesHnS : LogicUsables
{
	private HideAndSeekManager HnSManager;

	public LogicUsablesHnS(HideAndSeekManager manager)
		: base(manager)
	{
		HnSManager = manager;
	}

	public override bool CanUse(IUsable usable, PlayerControl player)
	{
		if (usable is Vent && player.Data.Role.IsImpostor)
		{
			return false;
		}
		return true;
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

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
